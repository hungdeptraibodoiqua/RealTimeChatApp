using System;
using System.Text;
using System.Threading.RateLimiting;
using ChatApp.API.Hubs;
using ChatApp.API.Middleware;
using ChatApp.Application;
using ChatApp.Infrastructure;
using ChatApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Đăng ký OpenAPI/Swagger để test endpoint.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Đăng ký controller để request HTTP từ client đi vào các file Controllers/* thay vì chỉ chạy endpoint tối giản.
builder.Services.AddControllers();

// Đăng ký SignalR để xử lý kết nối realtime giữa client và ChatHub.
builder.Services.AddSignalR();

// Cấu hình CORS cho phép frontend (port 3000 hoặc 5173) gọi API và kết nối WebSocket.
const string corsPolicyName = "AllowFrontendApp";
builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicyName, policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Bắt buộc cho SignalR để đính kèm cookie/thông tin xác thực
    });
});

// SEC-12: giới hạn tần suất cho đăng nhập/đăng ký để chống dò mật khẩu (brute force).
// Backend chỉ nhận request qua nginx trong mạng private; nginx ghi đè X-Real-IP bằng $remote_addr,
// nên dùng header này làm khóa phân vùng theo IP người dùng.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("auth", httpContext =>
    {
        var clientIp = httpContext.Request.Headers["X-Real-IP"].FirstOrDefault()
            ?? httpContext.Connection.RemoteIpAddress?.ToString()
            ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(clientIp, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        });
    });
});

// Đọc cấu hình JWT sớm để fail fast nếu thiếu secret/issuer/audience khi API khởi động.
var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("Configuration value 'Jwt:Secret' was not found.");
if (Encoding.UTF8.GetByteCount(jwtSecret) < 32)
    throw new InvalidOperationException("Jwt:Secret must be at least 32 bytes.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException("Configuration value 'Jwt:Issuer' was not found.");
var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException("Configuration value 'Jwt:Audience' was not found.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Các rule này đảm bảo access token đúng issuer/audience, còn hạn và ký bằng secret của hệ thống.
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ClockSkew = TimeSpan.Zero
        };

        // Trích xuất access token từ query string cho các request kết nối SignalR Hub qua WebSocket
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hub"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

// Nối API với lớp Application để controller sau này có thể gọi command/query handler và các contract nghiệp vụ.
builder.Services.AddApplication();
// Nối API với lớp Infrastructure để Application có thể nhận implementation repository/service/realtime từ project Infrastructure.
builder.Services.AddInfrastructure(builder.Configuration);

// Đăng ký ChatNotifier để gửi SignalR notifications từ Controller ra client
builder.Services.AddScoped<ChatApp.Application.Abstractions.Realtime.IChatNotifier, ChatApp.API.Services.ChatNotifier>();

var app = builder.Build();

// Local lab only: automatically apply existing EF Core migrations after DB readiness.
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Middleware này chặn exception phát sinh từ Controller/Application rồi chuẩn hóa response JSON trả ngược cho client.
app.UseMiddleware<ExceptionMiddleware>();
// Middleware này ghi lại request/response để theo dõi luồng dữ liệu HTTP từ client tới API và quay về client.
app.UseMiddleware<RequestLoggingMiddleware>();

// SEC-05: KHÔNG còn phục vụ /uploads công khai bằng UseStaticFiles().
// Ảnh/video được lưu ngoài wwwroot (Media:StoragePath) và tải qua GET /api/messages/{id}/media có kiểm tra thành viên phòng.

// Kích hoạt CORS trước khi xác thực để các preflight request OPTIONS không bị chặn
app.UseCors(corsPolicyName);

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Authentication phải chạy trước Authorization để HttpContext.User được dựng từ JWT trước khi kiểm tra quyền.
app.UseAuthentication();
app.UseAuthorization();

// SEC-12: áp dụng policy rate limit cho các endpoint gắn [EnableRateLimiting].
app.UseRateLimiter();

// Map toàn bộ endpoint trong Controllers/* để frontend hoặc tool test gọi vào đúng route nghiệp vụ sau này.
app.MapControllers();

// Map route SignalR Hub cho client kết nối realtime chat và presence
app.MapHub<ChatHub>("/hub/chat");

// Endpoint health dùng để kiểm tra API process đã khởi động xong trước khi nối thêm auth, room, message flow.
app.MapGet("/health", () => Results.Ok(new
{
    status = "ok",
    service = "ChatApp.API"
}));

app.Run();


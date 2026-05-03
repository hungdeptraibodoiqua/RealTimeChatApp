using System;
using System.Text;
using ChatApp.API.Middleware;
using ChatApp.Application;
using ChatApp.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Đăng ký OpenAPI/Swagger để test endpoint.
// builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Đăng ký controller để request HTTP từ client đi vào các file Controllers/* thay vì chỉ chạy endpoint tối giản.
builder.Services.AddControllers();

// Đọc cấu hình JWT sớm để fail fast nếu thiếu secret/issuer/audience khi API khởi động.
var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("Configuration value 'Jwt:Secret' was not found.");
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
    });

// Nối API với lớp Application để controller sau này có thể gọi command/query handler và các contract nghiệp vụ.
builder.Services.AddApplication();
// Nối API với lớp Infrastructure để Application có thể nhận implementation repository/service/realtime từ project Infrastructure.
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Middleware này chặn exception phát sinh từ Controller/Application rồi chuẩn hóa response JSON trả ngược cho client.
app.UseMiddleware<ExceptionMiddleware>();
// Middleware này ghi lại request/response để theo dõi luồng dữ liệu HTTP từ client tới API và quay về client.
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseHttpsRedirection();
// Authentication phải chạy trước Authorization để HttpContext.User được dựng từ JWT trước khi kiểm tra quyền.
app.UseAuthentication();
app.UseAuthorization();

// Map toàn bộ endpoint trong Controllers/* để frontend hoặc tool test gọi vào đúng route nghiệp vụ sau này.
app.MapControllers();

// Endpoint health dùng để kiểm tra API process đã khởi động xong trước khi nối thêm auth, room, message flow.
app.MapGet("/health", () => Results.Ok(new
{
    status = "ok",
    service = "ChatApp.API"
}));

app.Run();

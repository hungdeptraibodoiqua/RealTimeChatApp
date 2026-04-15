using ChatApp.API.Middleware;
using ChatApp.Application;
using ChatApp.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Đăng ký OpenAPI để lớp API có thể public tài liệu endpoint cho frontend/test tool gọi vào.
builder.Services.AddOpenApi();
// Đăng ký controller để request HTTP từ client đi vào các file Controllers/* thay vì chỉ chạy endpoint tối giản.
builder.Services.AddControllers();
// Nối API với lớp Application để controller sau này có thể gọi command/query handler và các contract nghiệp vụ.
builder.Services.AddApplication();
// Nối API với lớp Infrastructure để Application có thể nhận implementation repository/service/realtime từ project Infrastructure.
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // Chỉ bật tài liệu OpenAPI ở môi trường dev để kiểm thử flow request từ client vào backend dễ hơn.
    app.MapOpenApi();
}

// Middleware này chặn exception phát sinh từ Controller/Application rồi chuẩn hóa response JSON trả ngược cho client.
app.UseMiddleware<ExceptionMiddleware>();
// Middleware này ghi lại request/response để theo dõi luồng dữ liệu HTTP từ client tới API và quay về client.
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseHttpsRedirection();

// Map toàn bộ endpoint trong Controllers/* để frontend hoặc tool test gọi vào đúng route nghiệp vụ sau này.
app.MapControllers();
// Endpoint health dùng để kiểm tra API process đã khởi động xong trước khi nối thêm auth, room, message flow.
app.MapGet("/health", () => Results.Ok(new
{
    status = "ok",
    service = "ChatApp.API"
}));

app.Run();

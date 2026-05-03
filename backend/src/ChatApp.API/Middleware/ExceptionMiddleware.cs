using System.Text.Json;
using ChatApp.Application.Common.Exceptions;

namespace ChatApp.API.Middleware;

/// <summary>
/// Middleware chuẩn hóa exception từ Controller/Application thành JSON response nhất quán.
/// </summary>
public sealed class ExceptionMiddleware
{
    // JsonOptions này đảm bảo mọi lỗi từ API được serialize theo cùng một format JSON trước khi trả về frontend.
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Đẩy request sang middleware/controller tiếp theo; nếu có lỗi ở API/Application thì khối catch bên dưới sẽ xử lý.
            await _next(context);
        }
        catch (ValidationException ex)
        {
            // ValidationException thường đến từ lớp Application khi dữ liệu request từ client không đạt rule nghiệp vụ.
            _logger.LogWarning(ex, "Validation error on {Method} {Path}", context.Request.Method, context.Request.Path);

            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";

            // Dữ liệu lỗi đi từ validator/handler -> ExceptionMiddleware -> response JSON để frontend hiển thị theo field.
            var payload = new
            {
                title = "Validation failed",
                status = StatusCodes.Status400BadRequest,
                errors = ex.Errors
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(payload, JsonOptions));
        }
        catch (NotFoundException ex)
        {
            // NotFoundException phản ánh entity hoặc resource mà controller/handler cần xử lý không còn tồn tại trong hệ thống.
            _logger.LogInformation(ex, "Resource not found on {Method} {Path}", context.Request.Method, context.Request.Path);

            context.Response.StatusCode = StatusCodes.Status404NotFound;
            context.Response.ContentType = "application/json";

            // Dữ liệu lỗi đi từ Application handler -> middleware -> client để client biết resource nào không tìm thấy.
            var payload = new
            {
                title = "Resource not found",
                status = StatusCodes.Status404NotFound,
                detail = ex.Message
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(payload, JsonOptions));
        }
        catch (AppException ex)
        {
            // AppException dùng cho lỗi nghiệp vụ có chủ đích, ví dụ trạng thái thao tác không hợp lệ nhưng không phải lỗi hệ thống.
            _logger.LogWarning(ex, "Application error on {Method} {Path}", context.Request.Method, context.Request.Path);

            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";

            // Response này giúp frontend nhận được thông báo nghiệp vụ rõ ràng thay vì 500 chung chung.
            var payload = new
            {
                title = "Application error",
                status = StatusCodes.Status400BadRequest,
                detail = ex.Message
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(payload, JsonOptions));
        }
        catch (Exception ex)
        {
            // Khối này chặn lỗi ngoài dự kiến từ bất kỳ lớp nào phía sau middleware để API không rò rỉ stacktrace ra client.
            _logger.LogError(ex, "Unhandled error on {Method} {Path}", context.Request.Method, context.Request.Path);

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            // Dữ liệu đi từ exception hệ thống -> middleware -> JSON 500 tối giản để client biết request thất bại ở server.
            var payload = new
            {
                title = "Internal server error",
                status = StatusCodes.Status500InternalServerError
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(payload, JsonOptions));
        }
    }
}

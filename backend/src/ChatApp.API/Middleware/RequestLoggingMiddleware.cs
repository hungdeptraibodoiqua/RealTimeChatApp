namespace ChatApp.API.Middleware;

/// <summary>
/// Middleware ghi log method/path/status/duration cho mỗi HTTP request.
/// </summary>
public sealed class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Lưu thời điểm request vừa đi vào pipeline để tính thời gian xử lý từ API tới controller rồi quay lại response.
        var startedAt = DateTime.UtcNow;

        // Chuyển request xuống middleware/controller tiếp theo để luồng nghiệp vụ thực thi.
        await _next(context);

        // Sau khi pipeline trả response, middleware tính elapsed time để theo dõi hiệu năng nền của từng endpoint.
        var elapsedMs = (DateTime.UtcNow - startedAt).TotalMilliseconds;

        // Log này nối dữ liệu từ HttpContext request/response thành một dòng theo dõi cho API gateway, controller và handler về sau.
        _logger.LogInformation(
            "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs:0.000} ms",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            elapsedMs);
    }
}

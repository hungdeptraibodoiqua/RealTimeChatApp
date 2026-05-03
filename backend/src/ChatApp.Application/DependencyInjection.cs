using Microsoft.Extensions.DependencyInjection;
using ChatApp.Application.Features.Auth.Commands.Login;
using ChatApp.Application.Features.Auth.Commands.RefreshToken;
using ChatApp.Application.Features.Auth.Commands.Register;

namespace ChatApp.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        // Đây là điểm đăng ký mọi service mức Application như mediator, validation, pipeline behavior.
        // Program.cs gọi vào đây để request từ controller có thể đi tiếp tới command/query handler trong Application.
        // Hiện tại method để trống vì project chưa có handler/validator thật, nhưng giữ sẵn điểm nối an toàn cho bước foundation kế tiếp.
        services.AddScoped<RegisterCommandHandler>();
        services.AddScoped<LoginCommandHandler>();
        services.AddScoped<RefreshTokenCommandHandler>();

        return services;
    }
}

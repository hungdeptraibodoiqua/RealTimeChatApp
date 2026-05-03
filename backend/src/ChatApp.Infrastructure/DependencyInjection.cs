using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using ChatApp.Application.Abstractions.Persistence;
using ChatApp.Application.Abstractions.Security;
using ChatApp.Infrastructure.Persistence;
using ChatApp.Infrastructure.Persistence.Repositories;
using ChatApp.Infrastructure.Security;

namespace ChatApp.Infrastructure;

/// <summary>
/// Composition root của Infrastructure: đăng ký EF Core, repository, UnitOfWork và security service cho Application abstractions.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        // PresenceTracker giữ trạng thái online/offline theo connection để SignalR hub hoặc notifier dùng lại sau này.
        // Program.cs gọi AddInfrastructure để Application/API có thể lấy implementation từ Infrastructure qua DI container.
        // Dữ liệu presence sẽ đi từ Hub/Realtime service -> PresenceTracker -> notifier/controller nếu cần phản hồi trạng thái online.
        services.AddSingleton<PresenceTracker>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        // Infrastructure cung cấp implementation cụ thể cho các contract mà Application định nghĩa.
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IRefreshTokenGenerator, RefreshTokenGenerator>();

        return services;
    }
}

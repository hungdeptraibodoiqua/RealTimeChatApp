using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using ChatApp.Infrastructure.Persistence;

namespace ChatApp.Infrastructure;

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

        return services;
    }
}

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ChatApp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // PresenceTracker giữ trạng thái online/offline theo connection để SignalR hub hoặc notifier dùng lại sau này.
        // Program.cs gọi AddInfrastructure để Application/API có thể lấy implementation từ Infrastructure qua DI container.
        // Dữ liệu presence sẽ đi từ Hub/Realtime service -> PresenceTracker -> notifier/controller nếu cần phản hồi trạng thái online.
        services.AddSingleton<PresenceTracker>();

        return services;
    }
}

using FpvBattle.Logic.Features.Notifications.Channels;
using FpvBattle.Logic.Features.Notifications.Jobs;
using FpvBattle.Logic.Features.Notifications.Services;
using FpvBattle.Logic.Jobs;
using Microsoft.Extensions.DependencyInjection;

namespace FpvBattle.Logic.Features.Notifications;

public static class NotificationsServiceExtensions
{
    public static IServiceCollection AddNotifications(this IServiceCollection services)
    {
        services.AddScoped<UserNotificationService>();
        services.AddScoped<InAppNotificationService>();
        services.AddScoped<INotificationChannel, InAppNotificationChannel>();
        services.AddScoped<NotificationCleanupJob>();
        services.AddScoped<IJobRegistrar, NotificationsJobRegistrar>();
        return services;
    }
}

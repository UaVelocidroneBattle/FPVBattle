using FpvBattle.Application.Features.Notifications.Channels;
using FpvBattle.Application.Features.Notifications.Jobs;
using FpvBattle.Application.Features.Notifications.Services;
using FpvBattle.Application.Jobs;
using Microsoft.Extensions.DependencyInjection;

namespace FpvBattle.Application.Features.Notifications;

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

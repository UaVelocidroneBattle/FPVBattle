using Microsoft.Extensions.DependencyInjection;
using Veloci.Logic.Features.Notifications.Channels;
using Veloci.Logic.Features.Notifications.Jobs;
using Veloci.Logic.Features.Notifications.Services;
using Veloci.Logic.Jobs;

namespace Veloci.Logic.Features.Notifications;

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

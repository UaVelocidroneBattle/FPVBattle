using Microsoft.EntityFrameworkCore;
using Serilog;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.Features.Notifications.Channels;

namespace Veloci.Logic.Features.Notifications.Services;

/// <summary>
/// Delivery is best effort: a channel that fails is logged and skipped, so a notification
/// never brings down the event handler that raised it.
/// </summary>
public class UserNotificationService
{
    private static readonly ILogger Log = Serilog.Log.ForContext<UserNotificationService>();

    private readonly IEnumerable<INotificationChannel> _channels;
    private readonly IRepository<ApplicationUser> _users;

    public UserNotificationService(IEnumerable<INotificationChannel> channels, IRepository<ApplicationUser> users)
    {
        _channels = channels;
        _users = users;
    }

    public async Task NotifyAsync(IReadOnlyCollection<UserNotificationMessage> messages, CancellationToken ct)
    {
        if (messages.Count == 0)
            return;

        foreach (var channel in _channels)
        {
            try
            {
                await channel.SendAsync(messages, ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Log.Error(ex, "Channel {Channel} failed to deliver {Count} notifications",
                    channel.GetType().Name, messages.Count);
            }
        }
    }

    public async Task NotifyPilotsAsync(IReadOnlyCollection<PilotNotificationMessage> messages, CancellationToken ct)
    {
        if (messages.Count == 0)
            return;

        Dictionary<int, string> userIdsByPilot;

        try
        {
            userIdsByPilot = await GetUserIdsByPilotAsync(messages.Select(message => message.Pilot), ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Log.Error(ex, "Could not resolve users for {Count} pilot notifications", messages.Count);
            return;
        }

        var userMessages = messages
            .Where(message => userIdsByPilot.ContainsKey(message.Pilot.Id))
            .Select(message => new UserNotificationMessage(userIdsByPilot[message.Pilot.Id], message.Type, message.Text, message.CupId))
            .ToList();

        await NotifyAsync(userMessages, ct);
    }

    private async Task<Dictionary<int, string>> GetUserIdsByPilotAsync(IEnumerable<Pilot> pilots, CancellationToken ct)
    {
        var pilotIds = pilots.Select(pilot => pilot.Id).Distinct().ToList();

        return await _users
            .GetAll(user => user.PilotId != null && pilotIds.Contains(user.PilotId.Value))
            .ToDictionaryAsync(user => user.PilotId!.Value, user => user.Id, ct);
    }
}

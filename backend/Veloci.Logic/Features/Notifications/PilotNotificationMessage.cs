using Veloci.Data.Domain;

namespace Veloci.Logic.Features.Notifications;

/// <summary>
/// A notification addressed to whoever has linked the pilot. Pilots without a linked
/// user are silently skipped. <paramref name="CupId"/> is null for anything that is not
/// tied to a single cup.
/// </summary>
public record PilotNotificationMessage(Pilot Pilot, NotificationType Type, string Text, string? CupId = null);

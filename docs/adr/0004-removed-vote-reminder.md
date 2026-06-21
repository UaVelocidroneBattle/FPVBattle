# ADR-0004: Removed Vote Reminder Feature

## Status
Accepted

## Context
The bot previously sent periodic reminders to pilots asking them to rate the current competition track. This was implemented as a scheduled Hangfire job that published a `VoteReminder` notification, which the Discord (and Telegram message pool) handled by sending a random encouraging message to the cup channel.

## Decision
Removed the VoteReminder feature entirely on 2026-06-21.

## Rationale
The feature was no longer needed and added unnecessary scheduling complexity.

## What Was Removed

### Files deleted
- `Veloci.Logic/Notifications/VoteReminder.cs` — MediatR notification record: `public record VoteReminder(Competition Competition) : INotification;`

### Code removed

**`Veloci.Logic/Bot/ChatMessage.cs`**
- `VoteReminder` value from the `ChatMessageType` enum

**`Veloci.Logic/Services/CompetitionConductor.cs`**
```csharp
public async Task VoteReminder(string cupId)
{
    _log.Debug("Publishing vote reminder for cup {CupId}", cupId);

    var competition = await GetActiveCompetitionAsync(cupId);

    if (competition is null)
    {
        _log.Warning("No active competition found for vote reminder in cup {CupId}", cupId);
        return;
    }

    await _mediator.Publish(new VoteReminder(competition));
}
```

**`Veloci.Logic/Features/Cups/CupOptions.cs`** — property on `ScheduleOptions`:
```csharp
/// <summary>
/// Vote reminder time in HH:mm format (e.g., "14:30"). Optional - if not set, no vote reminder will be sent for this cup.
/// </summary>
public string? VoteReminderTime { get; set; }
```

**`Veloci.Logic/Features/Cups/Jobs/CupJobRegistrar.cs`** — job registration block:
```csharp
// Register vote reminder job (if configured)
if (!string.IsNullOrEmpty(cupOptions.Schedule.VoteReminderTime))
{
    if (TimeSpan.TryParse(cupOptions.Schedule.VoteReminderTime, out var voteReminderTime))
    {
        var reminderJobId = $"vote-reminder-{cupId}";
        var reminderCron = $"{voteReminderTime.Minutes} {voteReminderTime.Hours} * * *";
        RecurringJob.AddOrUpdate<CompetitionConductor>(
            reminderJobId,
            conductor => conductor.VoteReminder(cupId),
            reminderCron,
            new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });
        _log.Information("✅ Registered job {JobId} with cron '{Cron}' (UTC) for cup {CupId}",
            reminderJobId, reminderCron, cupId);
    }
    else
    {
        _log.Error("Invalid VoteReminderTime format '{VoteReminderTime}' for cup {CupId}. Expected HH:mm format.",
            cupOptions.Schedule.VoteReminderTime, cupId);
    }
}
```

**`Veloci.Logic/Bot/Discord/DiscordMessageEventHandler.cs`**
- `INotificationHandler<VoteReminder>` removed from the class interface list
- Handler method:
```csharp
public async Task Handle(VoteReminder notification, CancellationToken cancellationToken)
{
    var message = _chatMessages.GetRandomByType(ChatMessageType.VoteReminder);

    if (message is null)
    {
        return;
    }

    await _cupMessenger.SendMessageToCupAsync(notification.Competition.CupId, message.Text);
}
```

**`Veloci.Logic/Bot/Discord/DiscordChatMessages.cs`** — 8 English messages:
```csharp
Messages.Add(new ChatMessage(ChatMessageType.VoteReminder, "👌 Don't forget to rate the track"));
Messages.Add(new ChatMessage(ChatMessageType.VoteReminder, "Rating tracks matters 👆"));
Messages.Add(new ChatMessage(ChatMessageType.VoteReminder, "So, how was the track? 👆"));
Messages.Add(new ChatMessage(ChatMessageType.VoteReminder, "Your opinion matters 👆"));
Messages.Add(new ChatMessage(ChatMessageType.VoteReminder, "Rate the track if you haven't yet 👆"));
Messages.Add(new ChatMessage(ChatMessageType.VoteReminder, "Go vote 👆"));
Messages.Add(new ChatMessage(ChatMessageType.VoteReminder, "Got 10 seconds for a quick survey? 👆"));
Messages.Add(new ChatMessage(ChatMessageType.VoteReminder, "🧐 Rating the track is no harder than liking something on TikTok. Give it a try."));
```

**`Veloci.Logic/Bot/Telegram/TelegramChatMessages.cs`** — 9 Ukrainian messages:
```csharp
Messages.Add(new ChatMessage(ChatMessageType.VoteReminder, "👌 Не забудь оцінити трек"));
Messages.Add(new ChatMessage(ChatMessageType.VoteReminder, "Оцінювати треки важливо 👆"));
Messages.Add(new ChatMessage(ChatMessageType.VoteReminder, "Ну як тобі трек? 👆"));
Messages.Add(new ChatMessage(ChatMessageType.VoteReminder, "Твоя думка важлива 👆"));
Messages.Add(new ChatMessage(ChatMessageType.VoteReminder, "Оціни трек, якщо ще ні 👆"));
Messages.Add(new ChatMessage(ChatMessageType.VoteReminder, "Йди голосуй 👆"));
Messages.Add(new ChatMessage(ChatMessageType.VoteReminder, "Чи є у вас 10 секунд на невеличке опитування? 👆"));
Messages.Add(new ChatMessage(ChatMessageType.VoteReminder, "🧐 Оцінити трек не важче, ніж в TikTok лайкнути. Спробуй."));
Messages.Add(new ChatMessage(ChatMessageType.VoteReminder, "🧠 Навіть твоє пасивне «нормальний» — це теж фідбек."));
```

**`Veloci.Web/appsettings.json`** — removed from both `open-class` and `whoop-class` cup definitions:
```json
"VoteReminderTime": "19:35"
```

**`Veloci.Web/appsettings.schema.json`** — removed schema entry:
```json
"VoteReminderTime": {
  "type": ["string", "null"],
  "pattern": "^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$",
  "description": "Vote reminder time in HH:mm format (e.g., '14:30'). Optional - if not set, no vote reminder will be sent."
}
```

## How to Restore

1. Re-add `VoteReminder` to the `ChatMessageType` enum in `ChatMessage.cs`
2. Recreate `Veloci.Logic/Notifications/VoteReminder.cs`
3. Add `VoteReminder()` method back to `CompetitionConductor`
4. Restore `VoteReminderTime` property to `ScheduleOptions` in `CupOptions.cs`
5. Add the job registration block back to `CupJobRegistrar.RegisterJobsForCup()`
6. Add `INotificationHandler<VoteReminder>` to `DiscordMessageEventHandler` and restore the `Handle` method
7. Restore chat messages in `DiscordChatMessages` and `TelegramChatMessages`
8. Add `VoteReminderTime` back to `appsettings.json` and `appsettings.schema.json`

## Date
2026-06-21

## Participants
Viacheslav Brovko
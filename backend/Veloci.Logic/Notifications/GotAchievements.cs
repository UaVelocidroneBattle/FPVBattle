using MediatR;
using Veloci.Logic.Services.Achievements;

namespace Veloci.Logic.Notifications;

public record GotAchievements(AchievementCheckResults Results) : INotification;

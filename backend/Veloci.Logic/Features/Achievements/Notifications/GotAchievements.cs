using MediatR;
using Veloci.Logic.Features.Achievements.Services;

namespace Veloci.Logic.Features.Achievements.Notifications;

public record GotAchievements(AchievementCheckResults Results) : INotification;

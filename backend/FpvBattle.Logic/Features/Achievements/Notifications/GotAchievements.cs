using FpvBattle.Logic.Features.Achievements.Services;
using MediatR;

namespace FpvBattle.Logic.Features.Achievements.Notifications;

public record GotAchievements(AchievementCheckResults Results) : INotification;

using FpvBattle.Application.Features.Achievements.Services;
using MediatR;

namespace FpvBattle.Application.Features.Achievements.Notifications;

public record GotAchievements(AchievementCheckResults Results) : INotification;

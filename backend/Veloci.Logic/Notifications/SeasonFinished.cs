using MediatR;
using Veloci.Data.Domain;

namespace Veloci.Logic.Notifications;

public record SeasonFinished(List<SeasonResult> Results, string SeasonName, string[] Winners, byte[] Image) : INotification;

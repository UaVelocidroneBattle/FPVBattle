using MediatR;
using Veloci.Data.Domain;

namespace Veloci.Logic.Notifications;

public record VoteReminder(Competition Competition) : INotification;

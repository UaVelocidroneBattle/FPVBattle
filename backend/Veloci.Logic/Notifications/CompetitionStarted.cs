using MediatR;
using Veloci.Data.Domain;
using Veloci.Logic.Features.Cups;

namespace Veloci.Logic.Notifications;

public record CompetitionStarted(
    Competition Competition,
    Track Track,
    CupOptions CupOptions
) : INotification;

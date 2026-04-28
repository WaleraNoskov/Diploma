using CommunityToolkit.Mvvm.Messaging;
using ImageAnalysis.Domain.Base;
using ImageAnalysis.Infrastructure.Contracts;
using MediatR;

namespace Diploma.Services;

public class DomainEventHandler : INotificationHandler<DomainEventNotification>
{
    public Task Handle(DomainEventNotification notification, CancellationToken cancellationToken)
    {
        WeakReferenceMessenger.Default.Send(notification);
        return Task.CompletedTask;
    }
}
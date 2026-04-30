using CommunityToolkit.Mvvm.Messaging;
using Diploma.Contracts.Events;
using ImageAnalysis.Domain.Base;
using ImageAnalysis.Infrastructure.Contracts;
using MediatR;

namespace Diploma.Services;

public class DomainEventHandler : INotificationHandler<DomainEventNotification>
{
    public Task Handle(DomainEventNotification notification, CancellationToken cancellationToken)
    {
        WeakReferenceMessenger.Default.Send(new NewSessionNotification(notification));
        return Task.CompletedTask;
    }
}
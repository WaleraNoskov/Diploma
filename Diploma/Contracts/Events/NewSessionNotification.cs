using CommunityToolkit.Mvvm.Messaging.Messages;
using ImageAnalysis.Infrastructure.Contracts;

namespace Diploma.Contracts.Events;

public class NewSessionNotification(DomainEventNotification notification)
    : ValueChangedMessage<DomainEventNotification>(notification);
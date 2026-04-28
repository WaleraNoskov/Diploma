using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Diploma.Contracts.Events;

public class NewSessionOpened(Guid Id) : ValueChangedMessage<Guid>(Id);
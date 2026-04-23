## DATA FLOW (Command path):

ViewModel
→ IMediator.Send(Command)
→ ExceptionHandlingBehaviour   (outermost safety net)
→ LoggingBehaviour             (timing + structured logs)
→ ValidationBehaviour          (IValidator<TCommand>)
→ CommandHandler
→ IImageSessionRepository.GetByIdAsync()
→ IImageStorage.GetAsync()
→ IImageProcessor.ApplyAsync()       ← OpenCV here
→ IImageStorage.StoreAsync()
→ session.ApplyOperation()           ← Domain aggregate
→ IImageSessionRepository.UpdateAsync()
→ IDomainEventPublisher.PublishAndClearAsync()
→ IPublisher.Publish(DomainEventNotification)
→ INotificationHandler<>   (log, UI refresh, etc.)
← Result<ImageSessionDto>
ViewModel

## DATA FLOW (Service pipeline path — for ViewModel convenience):

ViewModel
→ ImageProcessingService.PrepareAndDetectContoursAsync()
→ ExecutePipelineAsync([Grayscale, GaussianBlur, Threshold])
→ IMediator.Send(ApplyOperationCommand) × 3
→ IMediator.Send(DetectContoursCommand)
← Result<IReadOnlyList<ContourDto>>
ViewModel
*/

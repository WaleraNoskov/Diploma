using ImageAnalysis.Application.Behaviours;
using ImageAnalysis.Application.Contracts;
using ImageAnalysis.Application.Services;
using ImageAnalysis.Infrastructure.Services;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ImageAnalysis.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registers the full application + infrastructure stack.
    ///
    /// Usage (Generic Host / WPF with Microsoft.Extensions.DependencyInjection):
    /// <code>
    ///   services.AddImageProcessingApplication(typeof(Program).Assembly);
    /// </code>
    /// </summary>
    public static IServiceCollection AddImageProcessingApplication(
        this IServiceCollection services,
        params System.Reflection.Assembly[] handlerAssemblies)
    {
        // ---- MediatR --------------------------------------------------------
        services.AddMediatR(cfg =>
        {
            // Register all IRequestHandler<,> from provided assemblies
            foreach (var assembly in handlerAssemblies)
                cfg.RegisterServicesFromAssembly(assembly);

            // Pipeline (registration order = execution order from outside in)
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ExceptionHandlingBehaviour<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehaviour<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
        });

        // ---- Validators (register all found in provided assemblies) ---------
        foreach (var assembly in handlerAssemblies)
        {
            var validatorTypes = assembly.GetTypes()
                .Where(t => !t.IsAbstract && !t.IsInterface)
                .SelectMany(t => t.GetInterfaces()
                    .Where(i => i.IsGenericType &&
                                i.GetGenericTypeDefinition() == typeof(IValidator<>))
                    .Select(i => (Interface: i, Implementation: t)));

            foreach (var (iface, impl) in validatorTypes)
                services.AddTransient(iface, impl);
        }

        // ---- Repositories & Storage -----------------------------------------
        services.AddSingleton<IImageSessionRepository, InMemoryImageSessionRepository>();
        services.AddSingleton<IImageStorage, InMemoryImageStorage>();
        services.AddSingleton<IUnitOfWork, InMemoryUnitOfWork>();

        // ---- Domain Event Publisher -----------------------------------------
        services.AddScoped<IDomainEventPublisher, MediatRDomainEventPublisher>();

        // ---- Application Services -------------------------------------------
        services.AddSingleton<ImageProcessingService>();

        // NOTE: IImageProcessor is NOT registered here — it lives in a separate
        // Infrastructure.OpenCv project to keep the OpenCV dependency isolated.
        // Register it there:
        //   services.AddSingleton<IImageProcessor, OpenCvImageProcessor>();

        return services;
    }
}
using MediatR.Behaviors.Logging.AspNetCore;
using Microsoft.Extensions.DependencyInjection;

namespace MediatR
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMediatorAspNetCoreLogging(this IServiceCollection services)
        {
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LogExecutionBehavior<,>));

            return services;
        }
    }
}

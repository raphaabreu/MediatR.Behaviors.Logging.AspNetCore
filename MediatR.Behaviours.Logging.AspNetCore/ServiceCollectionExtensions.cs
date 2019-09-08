using MediatR.Behaviours.Logging.AspNetCore;
using Microsoft.Extensions.DependencyInjection;

namespace MediatR
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMediatRLogging(this IServiceCollection services)
        {
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LogExecutionBehaviour<,>));

            return services;
        }
    }
}

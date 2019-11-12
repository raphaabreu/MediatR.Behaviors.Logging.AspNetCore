using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MediatR.Behaviors.Logging.AspNetCore
{
    public class LogExecutionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly ILogger<TRequest> _logger;

        public LogExecutionBehavior(ILogger<TRequest> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            var correlationId = Guid.NewGuid();
            var timer = new System.Diagnostics.Stopwatch();
            var data = JsonConvert.SerializeObject(request);
            using (var loggingScope = _logger.BeginScope("{MeditatorRequestName} with {MeditatorRequestData}, correlation id {CorrelationId}", typeof(TRequest).Name, data, correlationId))
            {
                try
                {
                    _logger.LogDebug("Handler for {MeditatorRequestName} starting", typeof(TRequest).Name, data);
                    timer.Start();
                    var result = await next();
                    timer.Stop();
                    _logger.LogDebug("Handler for {MeditatorRequestName} finished in {ElapsedMilliseconds}ms", typeof(TRequest).Name, data, timer.Elapsed.TotalMilliseconds);

                    return result;
                }
                catch (Exception e)
                {
                    timer.Stop();
                    _logger.LogError(e, "Handler for {MeditatorRequestName} failed in {ElapsedMilliseconds}ms", typeof(TRequest).Name, data, timer.Elapsed.TotalMilliseconds);
                    throw;
                }
            }
        }
    }
}

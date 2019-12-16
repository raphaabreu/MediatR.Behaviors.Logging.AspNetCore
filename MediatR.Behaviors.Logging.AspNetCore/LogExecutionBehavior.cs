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
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        public LogExecutionBehavior(ILogger<TRequest> logger, JsonSerializerSettings jsonSerializerSettings = null)
        {
            _logger = logger;
            _jsonSerializerSettings = jsonSerializerSettings ?? new JsonSerializerSettings();
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            var correlationId = Guid.NewGuid();
            var timer = new System.Diagnostics.Stopwatch();
            var data = JsonConvert.SerializeObject(request, _jsonSerializerSettings);
            using (var loggingScope = _logger.BeginScope("{MeditatorRequestName} with {MeditatorRequestData}, correlation id {CorrelationId}", typeof(TRequest).Name, data, correlationId))
            {
                try
                {
                    _logger.LogDebug("Handler for {MeditatorRequestName} starting", typeof(TRequest).Name);
                    timer.Start();
                    var result = await next();
                    timer.Stop();
                    _logger.LogDebug("Handler for {MeditatorRequestName} finished in {ElapsedMilliseconds}ms", typeof(TRequest).Name, timer.Elapsed.TotalMilliseconds);

                    return result;
                }
                catch (Exception e)
                {
                    timer.Stop();
                    _logger.LogError(e, "Handler for {MeditatorRequestName} failed in {ElapsedMilliseconds}ms", typeof(TRequest).Name, timer.Elapsed.TotalMilliseconds);
                    throw;
                }
            }
        }
    }
}

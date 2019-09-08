using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MediatR.Behaviours.Logging.AspNetCore
{
    public class LogExecutionBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly ILogger<TRequest> _logger;

        public LogExecutionBehaviour(ILogger<TRequest> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            var timer = new System.Diagnostics.Stopwatch();
            var data = JsonConvert.SerializeObject(request);
            using (var loggingScope = _logger.BeginScope("{MeditatorRequestName} with {MeditatorRequestData}", typeof(TRequest).Name, data))
            {
                try
                {
                    _logger.LogInformation("Handler for {MeditatorRequestName} with {MeditatorRequestData} starting", typeof(TRequest).Name, data);
                    timer.Start();
                    var result = await next();
                    timer.Stop();
                    _logger.LogInformation("Handler for {MeditatorRequestName} with {MeditatorRequestData} finished in {ElapsedMilliseconds}ms", typeof(TRequest).Name, data, timer.Elapsed.TotalMilliseconds);

                    return result;
                }
                catch (Exception e)
                {
                    timer.Stop();
                    _logger.LogError(e, "Handler for {MeditatorRequestName} with {MeditatorRequestData} failed in {ElapsedMilliseconds}ms", typeof(TRequest).Name, data, timer.Elapsed.TotalMilliseconds);
                    throw;
                }
            }
        }
    }
}

using MediatR;
using Microsoft.Extensions.Logging;

namespace ERP.Application.Common.Behaviours
{
    public class UnhandledExceptionBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    {
        private readonly ILogger<TRequest> _logger;

        public UnhandledExceptionBehaviour(ILogger<TRequest> logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            try
            {
                return await next();
            }
            catch (Exception ex)
            {
                var requestName = typeof(TRequest).Name;

                _logger.LogError(ex, "ERP Request: Unhandled Exception for Request {Name} {@Request}", requestName, request);

                throw;
            }
        }
    }
}

using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using ERP.Application.Common.Interfaces;

namespace ERP.Application.Common.Behaviours
{
    public class PerformanceBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    {
        private readonly Stopwatch _timer;
        private readonly ILogger<TRequest> _logger;
        private readonly ICurrentUserService _currentUserService;

        public PerformanceBehaviour(
            ILogger<TRequest> logger,
            ICurrentUserService currentUserService)
        {
            _timer = new Stopwatch();
            _logger = logger;
            _currentUserService = currentUserService;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            _timer.Start();

            var response = await next();

            _timer.Stop();

            var elapsedMilliseconds = _timer.ElapsedMilliseconds;

            // 500ms 이상 걸리는 요청은 경고 로그
            if (elapsedMilliseconds > 500)
            {
                var requestName = typeof(TRequest).Name;
                var identityUserId = _currentUserService.IdentityUserId;
                var businessUserId = 0;
                var userName = _currentUserService.UserName ?? string.Empty;

                // BusinessUserId 조회 시 예외 처리
                try
                {
                    if (_currentUserService.IsAuthenticated)
                    {
                        businessUserId = _currentUserService.BusinessUserId;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not retrieve BusinessUserId for performance logging");
                }

                _logger.LogWarning(
                    "ERP Long Running Request: {Name} ({ElapsedMilliseconds} milliseconds) {@IdentityUserId} {@BusinessUserId} {@UserName} {@Request}",
                    requestName, elapsedMilliseconds, identityUserId, businessUserId, userName, request);
            }

            return response;
        }
    }
}

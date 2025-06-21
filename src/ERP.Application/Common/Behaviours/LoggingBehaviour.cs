using ERP.Application.Common.Interfaces;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;

namespace ERP.Application.Common.Behaviours
{
    public class LoggingBehaviour<TRequest> : IRequestPreProcessor<TRequest>
        where TRequest : notnull
    {
        private readonly ILogger _logger;
        private readonly ICurrentUserService _currentUserService;

        public LoggingBehaviour(ILogger<TRequest> logger, ICurrentUserService currentUserService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        public async Task Process(TRequest request, CancellationToken cancellationToken)
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
                _logger.LogWarning(ex, "Could not retrieve BusinessUserId for logging");
            }

            _logger.LogInformation(
                "ERP Request: {Name} {@IdentityUserId} {@BusinessUserId} {@UserName} {@Request}",
                requestName, identityUserId, businessUserId, userName, request);

            await Task.CompletedTask;
        }
    }
}

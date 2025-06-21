using System.Text;
using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using ERP.Application.Common.Interfaces;

namespace ERP.Application.Common.Behaviours
{
    public class CachingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : ICacheableQuery
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<CachingBehaviour<TRequest, TResponse>> _logger;
        private readonly ICurrentUserService _currentUserService;

        public CachingBehaviour(
            IDistributedCache cache,
            ILogger<CachingBehaviour<TRequest, TResponse>> logger,
            ICurrentUserService currentUserService)
        {
            _cache = cache;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            // 캐싱이 비활성화되었으면 바로 다음 핸들러로
            if (!request.UseCache)
            {
                return await next();
            }

            var cacheKey = GenerateCacheKey(request);

            // 캐시에서 조회
            var cachedResponse = await _cache.GetStringAsync(cacheKey, cancellationToken);
            if (!string.IsNullOrEmpty(cachedResponse))
            {
                _logger.LogInformation("Cache hit for {CacheKey}", cacheKey);
                return JsonSerializer.Deserialize<TResponse>(cachedResponse)!;
            }

            _logger.LogInformation("Cache miss for {CacheKey}", cacheKey);

            // 캐시에 없으면 실행
            var response = await next();

            // 결과를 캐시에 저장
            var options = new DistributedCacheEntryOptions
            {
                SlidingExpiration = request.SlidingExpiration,
                AbsoluteExpiration = DateTimeOffset.UtcNow.Add(request.AbsoluteExpiration)
            };

            await _cache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(response),
                options,
                cancellationToken);

            return response;
        }

        private string GenerateCacheKey(TRequest request)
        {
            var requestName = request.GetType().Name;
            var companyId = _currentUserService.CompanyId;

            // request 객체를 JSON으로 직렬화하여 해시 생성
            var requestJson = JsonSerializer.Serialize(request);
            var requestHash = Convert.ToBase64String(
                System.Security.Cryptography.SHA256.HashData(
                    Encoding.UTF8.GetBytes(requestJson)));

            return $"ERP:{companyId}:{requestName}:{requestHash}";
        }
    }
}

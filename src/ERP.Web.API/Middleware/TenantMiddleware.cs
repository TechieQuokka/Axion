namespace ERP.Web.API.Middleware
{
    /// <summary>
    /// 서브도메인 기반 테넌트 식별 미들웨어
    /// </summary>
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TenantMiddleware> _logger;

        public TenantMiddleware(RequestDelegate next, ILogger<TenantMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // 서브도메인에서 테넌트 정보 추출
            var host = context.Request.Host.Host;
            var subdomain = ExtractSubdomain(host);

            if (!string.IsNullOrEmpty(subdomain))
            {
                // 테넌트 정보를 HttpContext.Items에 저장
                context.Items["TenantSubdomain"] = subdomain;
                _logger.LogDebug("Tenant identified: {Subdomain}", subdomain);
            }

            await _next(context);
        }

        private string ExtractSubdomain(string host)
        {
            // localhost나 IP 주소인 경우 처리
            if (host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
                System.Net.IPAddress.TryParse(host, out _))
            {
                return string.Empty;
            }

            var parts = host.Split('.');

            // 최소 3개 부분이 있어야 서브도메인이 존재 (subdomain.domain.com)
            if (parts.Length >= 3)
            {
                return parts[0];
            }

            return string.Empty;
        }
    }

    public static class TenantMiddlewareExtensions
    {
        public static IApplicationBuilder UseTenantMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TenantMiddleware>();
        }
    }
}

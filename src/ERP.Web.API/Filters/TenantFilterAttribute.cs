using ERP.Application.Common.Exceptions;
using ERP.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ERP.Web.API.Filters
{
    /// <summary>
    /// 멀티테넌트 환경에서 테넌트 검증을 위한 필터
    /// </summary>
    public class TenantFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var currentUserService = context.HttpContext.RequestServices
                .GetRequiredService<ICurrentUserService>();

            if (!currentUserService.IsAuthenticated)
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            // CompanyId가 유효한지 확인
            try
            {
                var companyId = currentUserService.CompanyId;
                if (companyId <= 0)
                {
                    throw new ForbiddenAccessException("Invalid company context.");
                }
            }
            catch (UnauthorizedAccessException)
            {
                throw new ForbiddenAccessException("Company context is required.");
            }

            base.OnActionExecuting(context);
        }
    }
}

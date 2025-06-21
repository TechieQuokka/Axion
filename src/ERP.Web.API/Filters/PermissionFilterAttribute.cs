using ERP.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ERP.Web.API.Filters
{
    /// <summary>
    /// 권한 기반 접근 제어를 위한 필터
    /// </summary>
    public class PermissionFilterAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string _permission;

        public PermissionFilterAttribute(string permission)
        {
            _permission = permission;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var currentUserService = context.HttpContext.RequestServices
                .GetRequiredService<ICurrentUserService>();

            if (!currentUserService.IsAuthenticated)
            {
                context.Result = new UnauthorizedResult();
                return;
            }
            if (!currentUserService.HasPermission(_permission))
            {
                context.Result = new ForbidResult();
                return;
            }
        }
    }
}

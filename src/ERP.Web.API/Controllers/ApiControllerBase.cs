using ERP.Application.Common.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public abstract class ApiControllerBase : ControllerBase
    {
        private ISender? _mediator;
        private ICurrentUserService? _currentUserService;

        protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();
        protected ICurrentUserService CurrentUser => _currentUserService ??= HttpContext.RequestServices.GetRequiredService<ICurrentUserService>();

        protected int CompanyId => CurrentUser.CompanyId;

        /// <summary>
        /// Identity system user ID (string)
        /// </summary>
        protected string IdentityUserId => CurrentUser.IdentityUserId;

        /// <summary>
        /// Business domain user ID (int)
        /// </summary>
        protected int BusinessUserId => CurrentUser.BusinessUserId;

        /// <summary>
        /// Business domain user ID (int) - 하위 호환성을 위해 유지
        /// </summary>
        protected int UserId => CurrentUser.UserId;

        protected string? UserName => CurrentUser.UserName;

        protected bool IsAuthenticated => CurrentUser.IsAuthenticated;

        /// <summary>
        /// 현재 사용자가 지정된 역할을 가지고 있는지 확인
        /// </summary>
        protected bool IsInRole(string role) => CurrentUser.IsInRole(role);

        /// <summary>
        /// 현재 사용자가 지정된 권한을 가지고 있는지 확인
        /// </summary>
        protected bool HasPermission(string permission) => CurrentUser.HasPermission(permission);

        /// <summary>
        /// 인증된 사용자인지 확인하고 아니면 UnauthorizedResult 반환
        /// </summary>
        protected IActionResult? EnsureAuthenticated()
        {
            if (!IsAuthenticated)
            {
                return Unauthorized();
            }
            return null;
        }

        /// <summary>
        /// 지정된 역할을 가지고 있는지 확인하고 아니면 ForbidResult 반환
        /// </summary>
        protected IActionResult? EnsureInRole(string role)
        {
            var authCheck = EnsureAuthenticated();
            if (authCheck != null) return authCheck;

            if (!IsInRole(role))
            {
                return Forbid();
            }
            return null;
        }

        /// <summary>
        /// 지정된 권한을 가지고 있는지 확인하고 아니면 ForbidResult 반환
        /// </summary>
        protected IActionResult? EnsureHasPermission(string permission)
        {
            var authCheck = EnsureAuthenticated();
            if (authCheck != null) return authCheck;

            if (!HasPermission(permission))
            {
                return Forbid();
            }
            return null;
        }
    }
}

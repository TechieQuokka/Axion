using ERP.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace ERP.Infrastructure.Identity
{
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string Permission { get; }

        public PermissionRequirement(string permission)
        {
            Permission = permission;
        }
    }

    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            var permission = context.User.FindFirst("permission")?.Value;
            if (permission != null && permission.Contains(requirement.Permission))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }

    // 회사별 리소스 접근 요구사항
    public class CompanyResourceRequirement : IAuthorizationRequirement
    {
        public string Resource { get; }

        public CompanyResourceRequirement(string resource)
        {
            Resource = resource;
        }
    }

    public class CompanyResourceAuthorizationHandler : AuthorizationHandler<CompanyResourceRequirement>
    {
        private readonly ICurrentUserService _currentUserService;

        public CompanyResourceAuthorizationHandler(ICurrentUserService currentUserService)
        {
            _currentUserService = currentUserService;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            CompanyResourceRequirement requirement)
        {
            var userCompanyId = _currentUserService.CompanyId;
            if (userCompanyId > 0)
            {
                // 여기서 리소스의 회사 ID를 확인하는 로직 구현
                // 예시로 성공 처리
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}

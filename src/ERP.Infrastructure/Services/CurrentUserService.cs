using ERP.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using ERP.Infrastructure.Identity;
using ERP.Infrastructure.Data;

namespace ERP.Infrastructure.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CurrentUserService> _logger;

        // 캐싱을 위한 필드들
        private int? _cachedBusinessUserId;
        private int? _cachedCompanyId;
        private string? _cachedUserName;
        private readonly object _lockObject = new();

        public CurrentUserService(
            IHttpContextAccessor httpContextAccessor,
            IServiceProvider serviceProvider,
            ILogger<CurrentUserService> logger)
        {
            _httpContextAccessor = httpContextAccessor ??
                throw new ArgumentNullException(nameof(httpContextAccessor));
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public string IdentityUserId =>
            _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        public int BusinessUserId
        {
            get
            {
                if (_cachedBusinessUserId.HasValue)
                    return _cachedBusinessUserId.Value;

                lock (_lockObject)
                {
                    if (_cachedBusinessUserId.HasValue)
                        return _cachedBusinessUserId.Value;

                    _cachedBusinessUserId = GetBusinessUserIdInternal();
                    return _cachedBusinessUserId.Value;
                }
            }
        }

        public int UserId => BusinessUserId; // 하위 호환성을 위해 유지

        public int CompanyId
        {
            get
            {
                if (_cachedCompanyId.HasValue)
                    return _cachedCompanyId.Value;

                lock (_lockObject)
                {
                    if (_cachedCompanyId.HasValue)
                        return _cachedCompanyId.Value;

                    _cachedCompanyId = GetCompanyIdInternal();
                    return _cachedCompanyId.Value;
                }
            }
        }

        public string UserName
        {
            get
            {
                if (_cachedUserName != null)
                    return _cachedUserName;

                lock (_lockObject)
                {
                    if (_cachedUserName != null)
                        return _cachedUserName;

                    _cachedUserName = GetUserNameInternal();
                    return _cachedUserName;
                }
            }
        }

        public bool IsAuthenticated =>
            _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

        public bool IsInRole(string role)
        {
            if (string.IsNullOrWhiteSpace(role))
                return false;

            return _httpContextAccessor.HttpContext?.User?.IsInRole(role) ?? false;
        }

        public bool HasPermission(string permission)
        {
            if (string.IsNullOrWhiteSpace(permission))
                return false;

            try
            {
                // 1. 직접 Permission Claim 확인
                var permissionClaim = _httpContextAccessor.HttpContext?.User?
                    .FindFirstValue("Permission");

                if (!string.IsNullOrEmpty(permissionClaim))
                {
                    var permissions = permissionClaim.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    if (permissions.Any(p => p.Trim().Equals(permission, StringComparison.OrdinalIgnoreCase)))
                        return true;
                }

                // 2. 여러 Permission Claims 확인
                var permissionClaims = _httpContextAccessor.HttpContext?.User?.FindAll("Permission");
                if (permissionClaims != null)
                {
                    if (permissionClaims.Any(claim =>
                        claim.Value.Equals(permission, StringComparison.OrdinalIgnoreCase)))
                        return true;
                }

                // 3. 역할 기반 권한 확인
                return CheckPermissionByRole(permission);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking permission: {Permission}", permission);
                return false;
            }
        }

        private int GetBusinessUserIdInternal()
        {
            try
            {
                var identityUserId = IdentityUserId;

                if (string.IsNullOrEmpty(identityUserId))
                {
                    _logger.LogDebug("No identity user ID found");
                    return 0;
                }

                // 먼저 Claim에서 BusinessUserId 확인
                var businessUserIdClaim = _httpContextAccessor.HttpContext?.User?
                    .FindFirstValue("BusinessUserId");

                if (int.TryParse(businessUserIdClaim, out var businessUserId) && businessUserId > 0)
                {
                    _logger.LogDebug("BusinessUserId found in claims: {BusinessUserId}", businessUserId);
                    return businessUserId;
                }

                // Claim에 없으면 DB에서 조회
                return GetBusinessUserIdFromDatabase(identityUserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting business user ID");
                return 0;
            }
        }

        private int GetBusinessUserIdFromDatabase(string identityUserId)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();

                // ApplicationDbContext를 직접 사용 (ApplicationUser 접근을 위해)
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                if (string.IsNullOrWhiteSpace(identityUserId))
                {
                    _logger.LogWarning("IdentityUserId is null or empty");
                    return 0;
                }

                _logger.LogDebug("Searching for ApplicationUser with IdentityUserId: {IdentityUserId}", identityUserId);

                // ApplicationUser에 직접 접근하기 위해 Set<ApplicationUser>() 사용
                var applicationUser = dbContext.Set<ApplicationUser>()
                    .Where(u => u.Id == identityUserId)  // ApplicationUser.Id (string) == identityUserId (string)
                    .Select(u => new { u.BusinessUserId, u.Email, u.CompanyId })
                    .FirstOrDefault();

                if (applicationUser == null)
                {
                    _logger.LogWarning("ApplicationUser not found for IdentityUserId: {IdentityUserId}", identityUserId);
                    return 0;
                }

                _logger.LogDebug("ApplicationUser found: Email={Email}, CompanyId={CompanyId}, BusinessUserId={BusinessUserId}",
                    applicationUser.Email, applicationUser.CompanyId, applicationUser.BusinessUserId);

                if (applicationUser.BusinessUserId.HasValue && applicationUser.BusinessUserId.Value > 0)
                {
                    _logger.LogDebug("BusinessUserId found from ApplicationUser: {BusinessUserId}",
                        applicationUser.BusinessUserId.Value);
                    return applicationUser.BusinessUserId.Value;
                }

                // BusinessUserId가 없으면 Email을 통한 매핑 시도
                if (!string.IsNullOrEmpty(applicationUser.Email))
                {
                    _logger.LogDebug("Attempting email mapping for: {Email} in CompanyId: {CompanyId}",
                        applicationUser.Email, applicationUser.CompanyId);

                    // IApplicationDbContext를 사용하여 비즈니스 사용자 조회
                    var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

                    // context.Users는 ERP.Domain.Entities.User를 의미하므로 Id는 int 타입
                    var businessUserId = context.Users
                        .Where(u => u.Email == applicationUser.Email &&
                                   u.CompanyId == applicationUser.CompanyId &&
                                   !u.IsDeleted)
                        .Select(u => u.Id)  // ERP.Domain.Entities.User.Id (int)
                        .FirstOrDefault();

                    if (businessUserId > 0)
                    {
                        _logger.LogDebug("BusinessUserId found by email mapping: {BusinessUserId}", businessUserId);

                        // 찾은 BusinessUserId를 ApplicationUser에 업데이트 (향후 빠른 조회를 위해)
                        Task.Run(async () =>
                        {
                            try
                            {
                                using var updateScope = _serviceProvider.CreateScope();
                                var updateDbContext = updateScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                                var appUser = await updateDbContext.Set<ApplicationUser>().FindAsync(identityUserId);
                                if (appUser != null)
                                {
                                    appUser.BusinessUserId = businessUserId;
                                    await updateDbContext.SaveChangesAsync();
                                    _logger.LogDebug("Updated ApplicationUser.BusinessUserId: {BusinessUserId}", businessUserId);
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Failed to update ApplicationUser.BusinessUserId");
                            }
                        });

                        return businessUserId;
                    }
                    else
                    {
                        _logger.LogWarning("No business user found with email: {Email} in CompanyId: {CompanyId}",
                            applicationUser.Email, applicationUser.CompanyId);
                    }
                }
                else
                {
                    _logger.LogWarning("ApplicationUser email is null or empty for IdentityUserId: {IdentityUserId}", identityUserId);
                }

                _logger.LogWarning("No business user found for identity user: {IdentityUserId}", identityUserId);
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying business user ID from database for IdentityUserId: {IdentityUserId}", identityUserId);
                return 0;
            }
        }

        private int GetCompanyIdInternal()
        {
            try
            {
                // 1. 먼저 Claim에서 확인
                var companyIdClaim = _httpContextAccessor.HttpContext?.User?
                    .FindFirstValue("CompanyId");

                if (int.TryParse(companyIdClaim, out var companyId) && companyId > 0)
                {
                    _logger.LogDebug("CompanyId found in claims: {CompanyId}", companyId);
                    return companyId;
                }

                // 2. ApplicationUser에서 조회
                var identityUserId = IdentityUserId;
                if (!string.IsNullOrEmpty(identityUserId))
                {
                    using var scope = _serviceProvider.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    _logger.LogDebug("Searching for CompanyId with IdentityUserId: {IdentityUserId}", identityUserId);

                    var companyIdFromDb = dbContext.Set<ApplicationUser>()  // ApplicationUser에 직접 접근
                        .Where(u => u.Id == identityUserId)  // ApplicationUser.Id (string) == identityUserId (string)
                        .Select(u => u.CompanyId)
                        .FirstOrDefault();

                    if (companyIdFromDb > 0)
                    {
                        _logger.LogDebug("CompanyId found in ApplicationUser: {CompanyId}", companyIdFromDb);
                        return companyIdFromDb;
                    }
                    else
                    {
                        _logger.LogWarning("ApplicationUser not found or CompanyId is 0 for IdentityUserId: {IdentityUserId}", identityUserId);
                    }
                }
                else
                {
                    _logger.LogWarning("IdentityUserId is null or empty");
                }

                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting company ID for IdentityUserId: {IdentityUserId}", IdentityUserId);
                return 0;
            }
        }

        private string GetUserNameInternal()
        {
            var name = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name);
            if (!string.IsNullOrEmpty(name))
                return name;

            var givenName = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.GivenName);
            var surname = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Surname);

            if (!string.IsNullOrEmpty(givenName) || !string.IsNullOrEmpty(surname))
                return $"{givenName} {surname}".Trim();

            var email = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);
            if (!string.IsNullOrEmpty(email))
                return email.Split('@')[0];

            return string.Empty;
        }

        private bool CheckPermissionByRole(string permission)
        {
            if (IsInRole("Admin") || IsInRole("Administrator"))
                return true;

            var rolePermissions = new Dictionary<string, string[]>
            {
                ["Manager"] = new[]
                {
                    "Projects.View", "Projects.Create", "Projects.Edit",
                    "Users.View", "Reports.View",
                    "Customers.View", "Customers.Create", "Customers.Edit"
                },
                ["ProjectManager"] = new[]
                {
                    "Projects.View", "Projects.Edit", "Projects.Manage",
                    "Tasks.View", "Tasks.Create", "Tasks.Edit", "Tasks.Delete",
                    "TimeEntries.View", "TimeEntries.Approve"
                },
                ["Developer"] = new[]
                {
                    "Projects.View", "Tasks.View", "Tasks.Edit",
                    "TimeEntries.View", "TimeEntries.Create", "TimeEntries.Edit"
                },
                ["Designer"] = new[]
                {
                    "Projects.View", "Tasks.View", "Tasks.Edit",
                    "TimeEntries.View", "TimeEntries.Create"
                },
                ["QA"] = new[]
                {
                    "Projects.View", "Tasks.View", "Tasks.Edit",
                    "TimeEntries.View", "TimeEntries.Create"
                },
                ["HR"] = new[]
                {
                    "Users.View", "Users.Create", "Users.Edit",
                    "Reports.View", "Reports.HR"
                },
                ["Finance"] = new[]
                {
                    "Invoices.View", "Invoices.Create", "Invoices.Edit",
                    "Reports.View", "Reports.Financial",
                    "Customers.View"
                }
            };

            var userRoles = _httpContextAccessor.HttpContext?.User?.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList() ?? new List<string>();

            foreach (var role in userRoles)
            {
                if (rolePermissions.TryGetValue(role, out var permissions))
                {
                    if (permissions.Contains(permission, StringComparer.OrdinalIgnoreCase))
                        return true;
                }
            }

            return false;
        }
    }
}
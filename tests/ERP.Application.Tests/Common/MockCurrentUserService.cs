using ERP.Application.Common.Interfaces;

namespace ERP.Application.Tests.Common
{
    public class MockCurrentUserService : ICurrentUserService
    {
        public string IdentityUserId { get; set; } = "test-identity-user-id";
        public int BusinessUserId { get; set; } = 1;
        public int UserId => BusinessUserId; // 하위 호환성을 위해
        public int CompanyId { get; set; } = 1;
        public string UserName { get; set; } = "Test User";
        public bool IsAuthenticated { get; set; } = true;

        private readonly HashSet<string> _roles = new();
        private readonly HashSet<string> _permissions = new();

        public void AddRole(string role) => _roles.Add(role);
        public void AddPermission(string permission) => _permissions.Add(permission);
        public void RemoveRole(string role) => _roles.Remove(role);
        public void RemovePermission(string permission) => _permissions.Remove(permission);
        public void ClearRoles() => _roles.Clear();
        public void ClearPermissions() => _permissions.Clear();

        public bool IsInRole(string role) => _roles.Contains(role);
        public bool HasPermission(string permission) => _permissions.Contains(permission);

        /// <summary>
        /// 테스트용 편의 메서드: 특정 사용자로 설정
        /// </summary>
        public void SetUser(string identityUserId, int businessUserId, int companyId, string userName = "Test User")
        {
            IdentityUserId = identityUserId;
            BusinessUserId = businessUserId;
            CompanyId = companyId;
            UserName = userName;
            IsAuthenticated = true;
        }

        /// <summary>
        /// 테스트용 편의 메서드: 인증되지 않은 사용자로 설정
        /// </summary>
        public void SetUnauthenticated()
        {
            IdentityUserId = string.Empty;
            BusinessUserId = 0;
            CompanyId = 0;
            UserName = string.Empty;
            IsAuthenticated = false;
            _roles.Clear();
            _permissions.Clear();
        }

        /// <summary>
        /// 테스트용 편의 메서드: 관리자 권한 설정
        /// </summary>
        public void SetAsAdmin()
        {
            AddRole("Admin");
            AddRole("Administrator");
            AddPermission("*"); // 모든 권한
        }

        /// <summary>
        /// 테스트용 편의 메서드: 프로젝트 매니저 권한 설정
        /// </summary>
        public void SetAsProjectManager()
        {
            AddRole("ProjectManager");
            AddPermission("Projects.View");
            AddPermission("Projects.Edit");
            AddPermission("Projects.Manage");
            AddPermission("Tasks.View");
            AddPermission("Tasks.Create");
            AddPermission("Tasks.Edit");
            AddPermission("Tasks.Delete");
            AddPermission("TimeEntries.View");
            AddPermission("TimeEntries.Approve");
        }

        /// <summary>
        /// 테스트용 편의 메서드: 개발자 권한 설정
        /// </summary>
        public void SetAsDeveloper()
        {
            AddRole("Developer");
            AddPermission("Projects.View");
            AddPermission("Tasks.View");
            AddPermission("Tasks.Edit");
            AddPermission("TimeEntries.View");
            AddPermission("TimeEntries.Create");
            AddPermission("TimeEntries.Edit");
        }
    }
}

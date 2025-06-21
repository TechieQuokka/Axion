using ERP.Domain.Common;
using ERP.Domain.Enums;

namespace ERP.Domain.Entities
{
    public class User : BaseAuditableEntity
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// 암호화된 비밀번호 해시 (필수 필드)
        /// </summary>
        public string PasswordHash { get; set; } = string.Empty;

        // NULL 안전 처리
        public string? Phone { get; set; }
        public string? EmployeeId { get; set; }
        public Department Department { get; set; }

        // NULL 안전 처리
        public string? Position { get; set; }
        public DateTime? HireDate { get; set; }
        public UserStatus Status { get; set; }
        public decimal? HourlyRate { get; set; }
        public decimal? MonthlySalary { get; set; }

        /// <summary>
        /// 프로필 이미지 경로 (선택적)
        /// </summary>
        public string? ProfileImage { get; set; }

        /// <summary>
        /// 보유 기술/스킬 (JSON 형태)
        /// </summary>
        public string? Skills { get; set; }

        /// <summary>
        /// 마지막 로그인 시간
        /// </summary>
        public DateTime? LastLoginAt { get; set; }

        // Navigation properties
        public virtual Company Company { get; set; } = new Company();
        public virtual ICollection<ProjectMember> ProjectMembers { get; set; }
        public virtual ICollection<ProjectTask> AssignedTasks { get; set; }
        public virtual ICollection<TimeEntry> TimeEntries { get; set; }
        public virtual ICollection<Project> ManagedProjects { get; set; }
        public virtual ICollection<Project> TechnicalLeadProjects { get; set; }

        public User()
        {
            ProjectMembers = new HashSet<ProjectMember>();
            AssignedTasks = new HashSet<ProjectTask>();
            TimeEntries = new HashSet<TimeEntry>();
            ManagedProjects = new HashSet<Project>();
            TechnicalLeadProjects = new HashSet<Project>();
        }

        /// <summary>
        /// 사용자 전체 이름
        /// </summary>
        public string FullName => $"{FirstName} {LastName}";

        /// <summary>
        /// 사용자가 활성 상태인지 확인
        /// </summary>
        public bool IsActive => Status == UserStatus.Active;

        /// <summary>
        /// 사용자 표시명 (이름 + 직급)
        /// </summary>
        public string DisplayName => !string.IsNullOrEmpty(Position)
            ? $"{FullName} ({Position})"
            : FullName;
    }
}

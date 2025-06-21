using ERP.Domain.Common;
using ERP.Domain.Enums;
using System.Reflection.Metadata;

namespace ERP.Domain.Entities
{
    public class Project : BaseAuditableEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public ProjectStatus Status { get; set; }
        public ProjectType Type { get; set; }
        public Priority Priority { get; set; }
        public decimal Budget { get; set; }
        public decimal ActualCost { get; set; }
        public int Progress { get; set; }

        // Foreign Keys
        public int CustomerId { get; set; }
        public int ProjectManagerId { get; set; }

        /// <summary>
        /// 기술 리더 ID (선택적)
        /// </summary>
        public int? TechnicalLeadId { get; set; }

        /// <summary>
        /// Git 저장소 URL (선택적)
        /// </summary>
        public string? Repository { get; set; }

        /// <summary>
        /// 사용 기술 스택 (JSON 형태)
        /// </summary>
        public string? Technologies { get; set; }

        // Navigation properties
        public virtual Company Company { get; set; } = new Company();
        public virtual Customer Customer { get; set; } = new Customer();
        public virtual User ProjectManager { get; set; } = new User();

        /// <summary>
        /// 기술 리더 (선택적)
        /// </summary>
        public virtual User? TechnicalLead { get; set; }

        public virtual ICollection<ProjectMember> Members { get; set; }
        public virtual ICollection<ProjectTask> Tasks { get; set; }
        public virtual ICollection<TimeEntry> TimeEntries { get; set; }
        public virtual ICollection<Invoice> Invoices { get; set; }

        public Project()
        {
            Members = new HashSet<ProjectMember>();
            Tasks = new HashSet<ProjectTask>();
            TimeEntries = new HashSet<TimeEntry>();
            Invoices = new HashSet<Invoice>();
        }

        /// <summary>
        /// 예산 초과 여부
        /// </summary>
        public bool IsOverBudget => ActualCost > Budget;

        /// <summary>
        /// 일정 지연 여부
        /// </summary>
        public bool IsDelayed => ActualEndDate > EndDate ||
                                (DateTime.Now > EndDate && Status != ProjectStatus.Completed);

        /// <summary>
        /// 프로젝트 진행 일수
        /// </summary>
        public int DaysElapsed => ActualStartDate.HasValue
            ? (DateTime.Now - ActualStartDate.Value).Days
            : 0;

        /// <summary>
        /// 예상 완료일까지 남은 일수
        /// </summary>
        public int DaysRemaining => (EndDate - DateTime.Now).Days;

        /// <summary>
        /// 프로젝트 상태가 활성인지 확인
        /// </summary>
        public bool IsActive => Status == ProjectStatus.InProgress || Status == ProjectStatus.Planning;

        /// <summary>
        /// 프로젝트 완료 여부
        /// </summary>
        public bool IsCompleted => Status == ProjectStatus.Completed;

        /// <summary>
        /// 예산 사용률 (%)
        /// </summary>
        public decimal BudgetUtilizationPercentage => Budget > 0
            ? Math.Round((ActualCost / Budget) * 100, 2)
            : 0;

        /// <summary>
        /// 프로젝트 기간 (총 일수)
        /// </summary>
        public int TotalDays => (EndDate - StartDate).Days;

        /// <summary>
        /// 프로젝트 표시명
        /// </summary>
        public string DisplayName => $"[{Code}] {Name}";
    }
}

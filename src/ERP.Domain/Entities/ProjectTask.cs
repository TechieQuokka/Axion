using ERP.Domain.Common;
using ERP.Domain.Enums;

namespace ERP.Domain.Entities
{
    public class ProjectTask : BaseAuditableEntity
    {
        public int ProjectId { get; set; }
        public int? ParentTaskId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Enums.TaskStatus Status { get; set; }
        public Priority Priority { get; set; }
        public decimal? EstimatedHours { get; set; }
        public decimal ActualHours { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public int? AssigneeId { get; set; }

        // Navigation properties
        public virtual Project Project { get; set; } = new Project();   
        public virtual ProjectTask ParentTask { get; set; } = new ProjectTask();
        public virtual User Assignee { get; set; } = new User();
        public virtual ICollection<ProjectTask> SubTasks { get; set; }
        public virtual ICollection<TimeEntry> TimeEntries { get; set; }

        public ProjectTask()
        {
            SubTasks = new HashSet<ProjectTask>();
            TimeEntries = new HashSet<TimeEntry>();
            Status = Enums.TaskStatus.ToDo;
            Priority = Priority.Medium;
            ActualHours = 0;
        }

        public bool IsOverdue => DueDate.HasValue && DueDate.Value < DateTime.Now && Status != Enums.TaskStatus.Done;
        public decimal RemainingHours => Math.Max(0, (EstimatedHours ?? 0) - ActualHours);
    }
}

using ERP.Domain.Common;
using ERP.Domain.Enums;

namespace ERP.Domain.Entities
{
    public class TimeEntry : BaseAuditableEntity
    {
        public int UserId { get; set; }
        public int ProjectId { get; set; }
        public int? TaskId { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Hours { get; set; }
        public DateTime Date { get; set; }
        public decimal? HourlyRate { get; set; }
        public TimeEntryStatus Status { get; set; }

        // Navigation properties
        public virtual User User { get; set; } = new User();
        public virtual Project Project { get; set; } = new Project();
        public virtual ProjectTask Task { get; set; } = new ProjectTask();

        public TimeEntry()
        {
            Status = TimeEntryStatus.Draft;
            Date = DateTime.Today;
        }

        public decimal TotalAmount => Hours * (HourlyRate ?? 0);
    }
}

using ERP.Domain.Common;
using ERP.Domain.Enums;

namespace ERP.Domain.Entities
{
    public class ProjectMember : BaseEntity
    {
        public int ProjectId { get; set; }
        public int UserId { get; set; }
        public ProjectRole Role { get; set; }
        public int AllocationPercentage { get; set; }
        public DateTime AssignedDate { get; set; }
        public DateTime? UnassignedDate { get; set; }
        public decimal? HourlyRate { get; set; }

        // Navigation properties
        public virtual Project Project { get; set; } = new Project();
        public virtual User User { get; set; } = new User();

        public ProjectMember()
        {
            AllocationPercentage = 100;
            AssignedDate = DateTime.UtcNow;
        }

        public bool IsActive => UnassignedDate == null || UnassignedDate > DateTime.UtcNow;
    }
}

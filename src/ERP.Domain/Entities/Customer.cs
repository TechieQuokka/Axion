using ERP.Domain.Common;
using ERP.Domain.Enums;

namespace ERP.Domain.Entities
{
    public class Customer : BaseAuditableEntity
    {
        public string Name { get; set; } = string.Empty;
        public string ContactName { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty; // JSON string for address
        public string BusinessNumber { get; set; } = string.Empty;
        public string Industry { get; set; } = string.Empty;
        public CustomerType Type { get; set; }
        public CustomerStatus Status { get; set; }

        // Navigation properties
        public virtual Company Company { get; set; } = new Company();
        public virtual ICollection<Project> Projects { get; set; }
        public virtual ICollection<Invoice> Invoices { get; set; }

        public Customer()
        {
            Projects = new HashSet<Project>();
            Invoices = new HashSet<Invoice>();
            Status = CustomerStatus.Active;
            Type = CustomerType.SME;
        }
    }
}

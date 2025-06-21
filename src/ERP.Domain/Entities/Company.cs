using ERP.Domain.Common;
using ERP.Domain.Enums;

namespace ERP.Domain.Entities
{
    public class Company
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Domain { get; set; } = string.Empty;

        // NULL 안전 처리
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }

        public SubscriptionPlan Plan { get; set; }
        public DateTime? SubscriptionExpiry { get; set; }

        // NULL 안전 처리
        public string? Settings { get; set; } // JSON string
        public string? Logo { get; set; }
        public string? Address { get; set; }
        public string? BusinessNumber { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }

        // Navigation properties
        public virtual ICollection<User> Users { get; set; }
        public virtual ICollection<Project> Projects { get; set; }
        public virtual ICollection<Customer> Customers { get; set; }
        public virtual ICollection<Invoice> Invoices { get; set; }

        public Company()
        {
            Users = new HashSet<User>();
            Projects = new HashSet<Project>();
            Customers = new HashSet<Customer>();
            Invoices = new HashSet<Invoice>();
        }
    }
}

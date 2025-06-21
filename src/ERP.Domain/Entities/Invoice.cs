using ERP.Domain.Common;
using ERP.Domain.Enums;

namespace ERP.Domain.Entities
{
    public class Invoice : BaseAuditableEntity
    {
        public int CustomerId { get; set; }
        public int? ProjectId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; }
        public DateTime DueDate { get; set; }
        public InvoiceStatus Status { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Total { get; set; }
        public string Notes { get; set; } = string.Empty;
        public DateTime? PaidDate { get; set; }

        // Navigation properties
        public virtual Company Company { get; set; } = new Company();
        public virtual Customer Customer { get; set; } = new Customer();
        public virtual Project Project { get; set; } = new Project();
        public virtual ICollection<InvoiceItem> Items { get; set; }

        public Invoice()
        {
            Items = new HashSet<InvoiceItem>();
            Status = InvoiceStatus.Draft;
            IssueDate = DateTime.Today;
            DueDate = DateTime.Today.AddDays(30);
            TaxAmount = 0;
        }

        public bool IsOverdue => Status == InvoiceStatus.Sent && DueDate < DateTime.Today;
        public int DaysOverdue => IsOverdue ? (DateTime.Today - DueDate).Days : 0;
    }
}

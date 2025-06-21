using ERP.Domain.Common;

namespace ERP.Domain.Entities
{
    public class InvoiceItem : BaseEntity
    {
        public int InvoiceId { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Amount { get; set; }

        // Navigation properties
        public virtual Invoice Invoice { get; set; } = new Invoice();

        public InvoiceItem()
        {
            Quantity = 1;
        }

        public void CalculateAmount()
        {
            Amount = Quantity * UnitPrice;
        }
    }
}

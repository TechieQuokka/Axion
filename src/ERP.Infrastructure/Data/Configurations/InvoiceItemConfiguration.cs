using ERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Data.Configurations
{
    public class InvoiceItemConfiguration : IEntityTypeConfiguration<InvoiceItem>
    {
        public void Configure(EntityTypeBuilder<InvoiceItem> builder)
        {
            builder.ToTable("InvoiceItems");

            builder.Property(t => t.Description)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(t => t.Quantity)
                .HasPrecision(10, 2)
                .IsRequired();

            builder.Property(t => t.UnitPrice)
                .HasPrecision(15, 2)
                .IsRequired();

            builder.Property(t => t.Amount)
                .HasPrecision(15, 2)
                .IsRequired();

            builder.HasOne(d => d.Invoice)
                .WithMany(p => p.Items)
                .HasForeignKey(d => d.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

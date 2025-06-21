using ERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Data.Configurations
{
    public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.ToTable("Customers");

            builder.Property(t => t.Name)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(t => t.ContactName)
                .HasMaxLength(100);

            builder.Property(t => t.ContactEmail)
                .HasMaxLength(100);

            builder.Property(t => t.ContactPhone)
                .HasMaxLength(20);

            builder.Property(t => t.BusinessNumber)
                .HasMaxLength(20);

            builder.Property(t => t.Industry)
                .HasMaxLength(100);

            builder.Property(t => t.Address)
                .HasColumnType("JSON");

            builder.HasIndex(c => new { c.CompanyId, c.Status })
                .HasDatabaseName("IX_Customers_Company_Status");

            builder.HasIndex(c => new { c.CompanyId, c.Type })
                .HasDatabaseName("IX_Customers_Company_Type");

            builder.HasOne(d => d.Company)
                .WithMany(p => p.Customers)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

using ERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERP.Infrastructure.Data.Configurations
{
    public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
    {
        public void Configure(EntityTypeBuilder<Invoice> builder)
        {
            builder.ToTable("Invoices");

            builder.Property(t => t.InvoiceNumber)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(t => t.SubTotal)
                .HasPrecision(15, 2)
                .IsRequired();

            builder.Property(t => t.TaxAmount)
                .HasPrecision(15, 2)
                .HasDefaultValue(0);

            builder.Property(t => t.Total)
                .HasPrecision(15, 2)
                .IsRequired();

            builder.HasIndex(i => new { i.CompanyId, i.InvoiceNumber })
                .IsUnique()
                .HasDatabaseName("UK_Invoices_Company_Number");

            builder.HasIndex(i => new { i.CompanyId, i.Status })
                .HasDatabaseName("IX_Invoices_Company_Status");

            builder.HasIndex(i => i.CustomerId)
                .HasDatabaseName("IX_Invoices_Customer");

            builder.HasIndex(i => i.ProjectId)
                .HasDatabaseName("IX_Invoices_Project");

            builder.HasIndex(i => i.DueDate)
                .HasDatabaseName("IX_Invoices_DueDate");

            builder.HasOne(d => d.Company)
                .WithMany(p => p.Invoices)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(d => d.Customer)
                .WithMany(p => p.Invoices)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(d => d.Project)
                .WithMany(p => p.Invoices)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}

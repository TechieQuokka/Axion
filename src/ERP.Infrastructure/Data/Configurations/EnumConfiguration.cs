using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ERP.Domain.Entities;
using ERP.Domain.Enums;

namespace ERP.Infrastructure.Data.Configurations
{
    public class EnumConfiguration : IEntityTypeConfiguration<User>, IEntityTypeConfiguration<Company>, IEntityTypeConfiguration<Customer>, IEntityTypeConfiguration<Project>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            // Department와 UserStatus를 문자열로 저장
            builder.Property(u => u.Department)
                .HasConversion<string>();

            builder.Property(u => u.Status)
                .HasConversion<string>();
        }

        public void Configure(EntityTypeBuilder<Company> builder)
        {
            // SubscriptionPlan을 문자열로 저장
            builder.Property(c => c.Plan)
                .HasConversion<string>();
        }

        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            // CustomerType과 CustomerStatus를 문자열로 저장
            builder.Property(c => c.Type)
                .HasConversion<string>();

            builder.Property(c => c.Status)
                .HasConversion<string>();
        }

        public void Configure(EntityTypeBuilder<Project> builder)
        {
            // ProjectStatus, ProjectType, Priority를 문자열로 저장
            builder.Property(p => p.Status)
                .HasConversion<string>();

            builder.Property(p => p.Type)
                .HasConversion<string>();

            builder.Property(p => p.Priority)
                .HasConversion<string>();
        }
    }
}

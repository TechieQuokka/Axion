using ERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Data.Configurations
{
    public class ProjectMemberConfiguration : IEntityTypeConfiguration<ProjectMember>
    {
        public void Configure(EntityTypeBuilder<ProjectMember> builder)
        {
            builder.ToTable("ProjectMembers");

            builder.Property(t => t.AllocationPercentage)
                .HasDefaultValue(100);

            builder.Property(t => t.HourlyRate)
                .HasPrecision(10, 2);

            builder.HasIndex(pm => new { pm.ProjectId, pm.UserId })
                .IsUnique()
                .HasDatabaseName("UK_ProjectMembers_Project_User");

            builder.HasIndex(pm => pm.UserId)
                .HasDatabaseName("IX_ProjectMembers_User");

            builder.HasIndex(pm => new { pm.ProjectId, pm.Role })
                .HasDatabaseName("IX_ProjectMembers_Project_Role");

            builder.HasOne(d => d.Project)
                .WithMany(p => p.Members)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(d => d.User)
                .WithMany(p => p.ProjectMembers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

using ERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Data.Configurations
{
    public class ProjectTaskConfiguration : IEntityTypeConfiguration<ProjectTask>
    {
        public void Configure(EntityTypeBuilder<ProjectTask> builder)
        {
            builder.ToTable("ProjectTasks");

            builder.Property(t => t.Title)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(t => t.EstimatedHours)
                .HasPrecision(6, 2);

            builder.Property(t => t.ActualHours)
                .HasPrecision(6, 2)
                .HasDefaultValue(0);

            builder.HasIndex(pt => new { pt.ProjectId, pt.Status })
                .HasDatabaseName("IX_ProjectTasks_Project_Status");

            builder.HasIndex(pt => new { pt.ProjectId, pt.AssigneeId })
                .HasDatabaseName("IX_ProjectTasks_Project_Assignee");

            builder.HasIndex(pt => new { pt.AssigneeId, pt.Status })
                .HasDatabaseName("IX_ProjectTasks_Assignee_Status");

            builder.HasIndex(pt => pt.ParentTaskId)
                .HasDatabaseName("IX_ProjectTasks_Parent");

            builder.HasOne(d => d.Project)
                .WithMany(p => p.Tasks)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(d => d.ParentTask)
                .WithMany(p => p.SubTasks)
                .HasForeignKey(d => d.ParentTaskId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(d => d.Assignee)
                .WithMany(p => p.AssignedTasks)
                .HasForeignKey(d => d.AssigneeId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}

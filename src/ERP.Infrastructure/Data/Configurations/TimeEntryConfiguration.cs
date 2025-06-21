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
    public class TimeEntryConfiguration : IEntityTypeConfiguration<TimeEntry>
    {
        public void Configure(EntityTypeBuilder<TimeEntry> builder)
        {
            builder.ToTable("TimeEntries");

            builder.Property(t => t.Hours)
                .HasPrecision(4, 2)
                .IsRequired();

            builder.Property(t => t.HourlyRate)
                .HasPrecision(10, 2);

            builder.HasIndex(te => new { te.UserId, te.Date })
                .HasDatabaseName("IX_TimeEntries_User_Date");

            builder.HasIndex(te => new { te.ProjectId, te.Date })
                .HasDatabaseName("IX_TimeEntries_Project_Date");

            builder.HasIndex(te => te.TaskId)
                .HasDatabaseName("IX_TimeEntries_Task");

            builder.HasIndex(te => te.Status)
                .HasDatabaseName("IX_TimeEntries_Status");

            builder.HasOne(d => d.User)
                .WithMany(p => p.TimeEntries)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(d => d.Project)
                .WithMany(p => p.TimeEntries)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(d => d.Task)
                .WithMany(p => p.TimeEntries)
                .HasForeignKey(d => d.TaskId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}

using ERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Common.Interfaces
{
    /// <summary>
    /// Represents the application database context contract, exposing DbSet properties for each entity and the SaveChanges operation.
    /// </summary>
    public interface IApplicationDbContext
    {
        /// <summary>
        /// Provides access to Company entities in the database.
        /// </summary>
        DbSet<Company> Companies { get; }

        /// <summary>
        /// Provides access to User entities in the database.
        /// </summary>
        DbSet<User> Users { get; }

        /// <summary>
        /// Provides access to Customer entities in the database.
        /// </summary>
        DbSet<Customer> Customers { get; }

        /// <summary>
        /// Provides access to Project entities in the database.
        /// </summary>
        DbSet<Project> Projects { get; }

        /// <summary>
        /// Provides access to ProjectMember entities in the database.
        /// </summary>
        DbSet<ProjectMember> ProjectMembers { get; }

        /// <summary>
        /// Provides access to ProjectTask entities in the database.
        /// </summary>
        DbSet<ProjectTask> ProjectTasks { get; }

        /// <summary>
        /// Provides access to TimeEntry entities in the database.
        /// </summary>
        DbSet<TimeEntry> TimeEntries { get; }

        /// <summary>
        /// Provides access to Invoice entities in the database.
        /// </summary>
        DbSet<Invoice> Invoices { get; }

        /// <summary>
        /// Persists all changes made in this context to the database.
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>The number of state entries written to the database.</returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
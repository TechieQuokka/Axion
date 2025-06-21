using ERP.Domain.Common;

namespace ERP.Domain.Interfaces
{
    /// <summary>
    /// Defines a unit of work contract to coordinate changes across multiple repositories,
    /// ensuring transactional consistency.
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Gets the repository instance for a specific entity type.
        /// </summary>
        /// <typeparam name="T">Entity type that inherits from BaseEntity.</typeparam>
        IRepository<T> Repository<T>() where T : BaseEntity;

        /// <summary>
        /// Commits all tracked changes to the database asynchronously.
        /// </summary>
        /// <returns>The number of affected rows.</returns>
        Task<int> CompleteAsync();

        /// <summary>
        /// Commits all tracked changes with cancellation token support.
        /// </summary>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>The number of affected rows.</returns>
        Task<int> CompleteAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Rolls back the current transaction or changes in progress, if supported.
        /// </summary>
        Task RollbackAsync();
    }
}

using System.Linq.Expressions;
using ERP.Domain.Common;

namespace ERP.Domain.Interfaces
{
    /// <summary>
    /// Generic repository interface that defines common asynchronous data access operations for an entity.
    /// </summary>
    /// <typeparam name="T">The type of the entity, which must inherit from BaseEntity.</typeparam>
    public interface IRepository<T> where T : BaseEntity
    {
        /// <summary>
        /// Retrieves an entity by its unique identifier.
        /// </summary>
        Task<T> GetByIdAsync(int id);

        /// <summary>
        /// Retrieves all entities of type T.
        /// </summary>
        Task<IReadOnlyList<T>> GetAllAsync();

        /// <summary>
        /// Retrieves entities that match the specified predicate.
        /// </summary>
        Task<IReadOnlyList<T>> GetAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Retrieves entities with optional filtering, ordering, and related entity inclusion using a navigation string.
        /// </summary>
        Task<IReadOnlyList<T>> GetAsync(
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            string? includeString = null,
            bool disableTracking = true);

        /// <summary>
        /// Retrieves entities with optional filtering, ordering, and related entity inclusion using expression trees.
        /// </summary>
        Task<IReadOnlyList<T>> GetAsync(
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            List<Expression<Func<T, object>>>? includes = null,
            bool disableTracking = true);

        /// <summary>
        /// Adds a new entity to the repository asynchronously.
        /// </summary>
        Task<T> AddAsync(T entity);

        /// <summary>
        /// Updates an existing entity in the repository asynchronously.
        /// </summary>
        Task UpdateAsync(T entity);

        /// <summary>
        /// Deletes an entity from the repository asynchronously.
        /// </summary>
        Task DeleteAsync(T entity);

        /// <summary>
        /// Returns the number of entities that match the given predicate.
        /// </summary>
        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);

        /// <summary>
        /// Checks whether any entity satisfies the specified condition.
        /// </summary>
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
    }
}

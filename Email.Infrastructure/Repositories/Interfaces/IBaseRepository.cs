using System.Linq.Expressions;

namespace Email.Infrastructure.Repositories.Interfaces;

/// <summary>
/// Generic base repository interface for common CRUD operations.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public interface IBaseRepository<T> where T : class
{
    /// <summary>
    /// Gets an entity by its ID.
    /// </summary>
    /// <param name="id">The entity ID.</param>
    /// <returns>The entity if found, null otherwise.</returns>
    Task<T?> GetByIdAsync(long id);

    /// <summary>
    /// Gets all entities.
    /// </summary>
    /// <returns>A collection of all entities.</returns>
    Task<IEnumerable<T>> GetAllAsync();

    /// <summary>
    /// Gets entities based on a predicate.
    /// </summary>
    /// <param name="predicate">The filter predicate.</param>
    /// <returns>A collection of matching entities.</returns>
    Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// Adds a new entity.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <returns>The added entity with generated ID.</returns>
    Task<T> AddAsync(T entity);

    /// <summary>
    /// Updates an existing entity.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <returns>The updated entity.</returns>
    Task<T> UpdateAsync(T entity);

    /// <summary>
    /// Deletes an entity by ID (soft delete).
    /// </summary>
    /// <param name="id">The entity ID.</param>
    /// <returns>True if deleted successfully, false otherwise.</returns>
    Task<bool> DeleteAsync(long id);

    /// <summary>
    /// Checks if an entity exists by ID.
    /// </summary>
    /// <param name="id">The entity ID.</param>
    /// <returns>True if exists, false otherwise.</returns>
    Task<bool> ExistsAsync(long id);

    /// <summary>
    /// Gets the count of entities.
    /// </summary>
    /// <returns>The total count.</returns>
    Task<int> CountAsync();
}

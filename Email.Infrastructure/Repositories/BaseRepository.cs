using System.Data;
using System.Linq.Expressions;
using Dapper;
using Email.Infrastructure.Connection;
using Email.Infrastructure.Repositories.Interfaces;

namespace Email.Infrastructure.Repositories;

/// <summary>
/// Abstract base repository implementation using Dapper.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public abstract class BaseRepository<T> : IBaseRepository<T> where T : class
{
    protected readonly IDbConnectionFactory _connectionFactory;
    protected readonly string _tableName;

    protected BaseRepository(IDbConnectionFactory connectionFactory, string tableName)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _tableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
    }

    /// <summary>
    /// Gets an entity by its ID.
    /// </summary>
    public virtual async Task<T?> GetByIdAsync(long id)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = $"SELECT * FROM {_tableName} WHERE Id = @Id AND IsDeleted = 0";
        return await connection.QueryFirstOrDefaultAsync<T>(sql, new { Id = id });
    }

    /// <summary>
    /// Gets all entities.
    /// </summary>
    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = $"SELECT * FROM {_tableName} WHERE IsDeleted = 0 ORDER BY CreatedAt DESC";
        return await connection.QueryAsync<T>(sql);
    }

    /// <summary>
    /// Gets entities based on a predicate.
    /// Note: This is a simplified implementation. For complex predicates, consider using a query builder.
    /// </summary>
    public virtual async Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>> predicate)
    {
        // This is a simplified implementation. In a real scenario, you might want to use a query builder
        // or implement specific methods for common query patterns.
        throw new NotImplementedException("Complex predicates are not supported in this base implementation. Override this method in derived classes.");
    }

    /// <summary>
    /// Adds a new entity.
    /// </summary>
    public virtual async Task<T> AddAsync(T entity)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var transaction = connection.BeginTransaction();

        try
        {
            var sql = GetInsertSql();
            
            var id = await connection.ExecuteScalarAsync<long>(sql, entity, transaction);
            
            // Set the generated ID on the entity
            var idProperty = typeof(T).GetProperty("Id");
            if (idProperty != null)
            {
                idProperty.SetValue(entity, id);
            }

            transaction.Commit();
            return entity;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    /// <summary>
    /// Updates an existing entity.
    /// </summary>
    public virtual async Task<T> UpdateAsync(T entity)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var transaction = connection.BeginTransaction();

        try
        {
            var sql = GetUpdateSql();
            var rowsAffected = await connection.ExecuteAsync(sql, entity, transaction);
            
            if (rowsAffected == 0)
            {
                throw new InvalidOperationException("Entity not found or no changes made.");
            }

            transaction.Commit();
            return entity;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    /// <summary>
    /// Deletes an entity by ID (soft delete).
    /// </summary>
    public virtual async Task<bool> DeleteAsync(long id)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = $"UPDATE {_tableName} SET IsDeleted = 1, UpdatedAt = @UpdatedAt WHERE Id = @Id";
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id, UpdatedAt = DateTime.UtcNow });
        return rowsAffected > 0;
    }

    /// <summary>
    /// Checks if an entity exists by ID.
    /// </summary>
    public virtual async Task<bool> ExistsAsync(long id)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = $"SELECT COUNT(1) FROM {_tableName} WHERE Id = @Id AND IsDeleted = 0";
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Id = id });
        return count > 0;
    }

    /// <summary>
    /// Gets the count of entities.
    /// </summary>
    public virtual async Task<int> CountAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = $"SELECT COUNT(1) FROM {_tableName} WHERE IsDeleted = 0";
        return await connection.ExecuteScalarAsync<int>(sql);
    }

    /// <summary>
    /// Gets the SQL for inserting an entity.
    /// </summary>
    protected abstract string GetInsertSql();

    /// <summary>
    /// Gets the SQL for updating an entity.
    /// </summary>
    protected abstract string GetUpdateSql();
}

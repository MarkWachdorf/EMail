using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Email.Infrastructure.Entities;

/// <summary>
/// Base entity class containing common properties for all entities
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Timestamp when the record was created
    /// </summary>
    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the record was last updated
    /// </summary>
    [Column("UpdatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Soft delete flag
    /// </summary>
    [Column("IsDeleted")]
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Row version for optimistic concurrency control
    /// </summary>
    [Timestamp]
    [Column("RowVersion")]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// User or service that created the record
    /// </summary>
    [StringLength(100)]
    [Column("CreatedBy")]
    public string? CreatedBy { get; set; }

    /// <summary>
    /// User or service that last updated the record
    /// </summary>
    [StringLength(100)]
    [Column("UpdatedBy")]
    public string? UpdatedBy { get; set; }
}

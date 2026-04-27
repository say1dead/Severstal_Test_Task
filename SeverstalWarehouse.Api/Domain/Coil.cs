namespace SeverstalWarehouse.Api.Domain;

public sealed class Coil
{
    public Coil(long id, decimal length, decimal weight, DateTimeOffset addedAt, DateTimeOffset? removedAt)
    {
        Id = id;
        Length = length;
        Weight = weight;
        AddedAt = addedAt;
        RemovedAt = removedAt;
    }

    /// <summary>
    /// Coil identifier.
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    /// Coil length.
    /// </summary>
    public decimal Length { get; init; }

    /// <summary>
    /// Coil weight.
    /// </summary>
    public decimal Weight { get; init; }

    /// <summary>
    /// Date and time when the coil was added to the warehouse.
    /// </summary>
    public DateTimeOffset AddedAt { get; init; }

    /// <summary>
    /// Date and time when the coil was removed from the warehouse. Null means the coil is still in stock.
    /// </summary>
    public DateTimeOffset? RemovedAt { get; init; }
}


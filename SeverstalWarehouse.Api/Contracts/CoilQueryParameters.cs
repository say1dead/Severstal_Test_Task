namespace SeverstalWarehouse.Api.Contracts;

public sealed class CoilQueryParameters
{
    /// <summary>
    /// Minimum coil id.
    /// </summary>
    public long? IdFrom { get; init; }

    /// <summary>
    /// Maximum coil id.
    /// </summary>
    public long? IdTo { get; init; }

    /// <summary>
    /// Minimum coil weight.
    /// </summary>
    public decimal? WeightFrom { get; init; }

    /// <summary>
    /// Maximum coil weight.
    /// </summary>
    public decimal? WeightTo { get; init; }

    /// <summary>
    /// Minimum coil length.
    /// </summary>
    public decimal? LengthFrom { get; init; }

    /// <summary>
    /// Maximum coil length.
    /// </summary>
    public decimal? LengthTo { get; init; }

    /// <summary>
    /// Earliest coil addition date.
    /// </summary>
    public DateTimeOffset? AddedFrom { get; init; }

    /// <summary>
    /// Latest coil addition date.
    /// </summary>
    public DateTimeOffset? AddedTo { get; init; }

    /// <summary>
    /// Earliest coil removal date.
    /// </summary>
    public DateTimeOffset? RemovedFrom { get; init; }

    /// <summary>
    /// Latest coil removal date.
    /// </summary>
    public DateTimeOffset? RemovedTo { get; init; }

    /// <summary>
    /// When true, returns only coils currently in stock. When false, returns only removed coils.
    /// </summary>
    public bool? OnlyInStock { get; init; }
}


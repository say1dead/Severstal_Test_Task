namespace SeverstalWarehouse.Api.Domain;

public sealed class CoilFilter
{
    public long? IdFrom { get; init; }
    public long? IdTo { get; init; }
    public decimal? WeightFrom { get; init; }
    public decimal? WeightTo { get; init; }
    public decimal? LengthFrom { get; init; }
    public decimal? LengthTo { get; init; }
    public DateTimeOffset? AddedFrom { get; init; }
    public DateTimeOffset? AddedTo { get; init; }
    public DateTimeOffset? RemovedFrom { get; init; }
    public DateTimeOffset? RemovedTo { get; init; }
    public bool? OnlyInStock { get; init; }
}


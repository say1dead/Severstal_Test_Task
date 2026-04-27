namespace SeverstalWarehouse.Api.Domain;

public sealed record CoilStatistics(
    DateTimeOffset From,
    DateTimeOffset To,
    int AddedCount,
    int RemovedCount,
    decimal? AverageLength,
    decimal? AverageWeight,
    decimal? MinLength,
    decimal? MaxLength,
    decimal? MinWeight,
    decimal? MaxWeight,
    decimal TotalWeight,
    double? MinStorageDurationHours,
    double? MaxStorageDurationHours,
    DateOnly? MinCoilsCountDate,
    DateOnly? MaxCoilsCountDate,
    DateOnly? MinTotalWeightDate,
    DateOnly? MaxTotalWeightDate);
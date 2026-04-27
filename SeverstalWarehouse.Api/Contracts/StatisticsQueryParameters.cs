namespace SeverstalWarehouse.Api.Contracts;

public sealed class StatisticsQueryParameters
{
    /// <summary>
    /// Statistics period start.
    /// </summary>
    public DateTimeOffset? From { get; init; }

    /// <summary>
    /// Statistics period end.
    /// </summary>
    public DateTimeOffset? To { get; init; }
}


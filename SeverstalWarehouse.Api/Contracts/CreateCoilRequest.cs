namespace SeverstalWarehouse.Api.Contracts;

public sealed class CreateCoilRequest
{
    /// <summary>
    /// Coil length. Required, must be greater than zero.
    /// </summary>
    /// <example>1</example>
    public decimal Length { get; init; }

    /// <summary>
    /// Coil weight. Required, must be greater than zero.
    /// </summary>
    /// <example>20</example>
    public decimal Weight { get; init; }
}


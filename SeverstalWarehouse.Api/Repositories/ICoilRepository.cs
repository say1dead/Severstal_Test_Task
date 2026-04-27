using SeverstalWarehouse.Api.Domain;

namespace SeverstalWarehouse.Api.Repositories;

public interface ICoilRepository
{
    Task<Coil> AddAsync(decimal length, decimal weight, DateTimeOffset addedAt, CancellationToken cancellationToken);

    Task<Coil?> MarkRemovedAsync(long id, DateTimeOffset removedAt, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Coil>> GetAsync(CoilFilter filter, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Coil>> GetOverlappingPeriodAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken);
}


using SeverstalWarehouse.Api.Domain;

namespace SeverstalWarehouse.Api.Services;

public interface ICoilService
{
    Task<Coil> AddAsync(decimal length, decimal weight, CancellationToken cancellationToken);

    Task<Coil> RemoveAsync(long id, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Coil>> GetAsync(CoilFilter filter, CancellationToken cancellationToken);

    Task<CoilStatistics> GetStatisticsAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken);
}


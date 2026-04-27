using SeverstalWarehouse.Api.Domain;
using SeverstalWarehouse.Api.Repositories;

namespace SeverstalWarehouse.Tests.Mocks;

internal sealed class MockCoilRepository : ICoilRepository
{
    public Func<decimal, decimal, DateTimeOffset, CancellationToken, Task<Coil>>? AddAsyncMock { get; init; }

    public Func<long, DateTimeOffset, CancellationToken, Task<Coil?>>? MarkRemovedAsyncMock { get; init; }

    public Func<CoilFilter, CancellationToken, Task<IReadOnlyCollection<Coil>>>? GetAsyncMock { get; init; }

    public Func<DateTimeOffset, DateTimeOffset, CancellationToken, Task<IReadOnlyCollection<Coil>>>?
        GetOverlappingPeriodAsyncMock { get; init; }

    public Task<Coil> AddAsync(
        decimal length,
        decimal weight,
        DateTimeOffset addedAt,
        CancellationToken cancellationToken) =>
        AddAsyncMock?.Invoke(length, weight, addedAt, cancellationToken)
        ?? Task.FromResult(new Coil(1, length, weight, addedAt, null));

    public Task<Coil?> MarkRemovedAsync(long id, DateTimeOffset removedAt, CancellationToken cancellationToken) =>
        MarkRemovedAsyncMock?.Invoke(id, removedAt, cancellationToken)
        ?? Task.FromResult<Coil?>(new Coil(id, 100, 200, removedAt.AddDays(-1), removedAt));

    public Task<IReadOnlyCollection<Coil>> GetAsync(CoilFilter filter, CancellationToken cancellationToken) =>
        GetAsyncMock?.Invoke(filter, cancellationToken)
        ?? Task.FromResult<IReadOnlyCollection<Coil>>(Array.Empty<Coil>());

    public Task<IReadOnlyCollection<Coil>> GetOverlappingPeriodAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken) =>
        GetOverlappingPeriodAsyncMock?.Invoke(from, to, cancellationToken)
        ?? Task.FromResult<IReadOnlyCollection<Coil>>(Array.Empty<Coil>());
}

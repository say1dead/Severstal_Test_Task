using System.Reflection;
using System.Runtime;
using SeverstalWarehouse.Api.Domain;
using SeverstalWarehouse.Api.Exceptions;
using SeverstalWarehouse.Api.Repositories;
using SeverstalWarehouse.Api.Services;
using SeverstalWarehouse.Tests.Mocks;

namespace SeverstalWarehouse.Tests;

public sealed class CoilServiceTests
{
    [Fact]
    public async Task AddAsync_WithNonPositiveLength_ThrowsArgumentException()
    {
        var service = new CoilService(new MockCoilRepository());

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.AddAsync(0, 120, CancellationToken.None));
    }

    [Fact]
    public async Task AddAsync_WithNonPositiveWeight_ThrowsArgumentException()
    {
        var service = new CoilService(new MockCoilRepository());

        await Assert.ThrowsAsync<ArgumentException>(() => service.AddAsync(100, 0, CancellationToken.None));
    }

    [Fact]
    public async Task RemoveAsync_WhenCoilDoesNotExist_ThrowsNotFoundException()
    {
        var service = new CoilService(new MockCoilRepository
        {
            MarkRemovedAsyncMock = (_, _, _) => Task.FromResult<Coil?>(null)
        });

        await Assert.ThrowsAsync<CoilNotFoundException>(() =>
            service.RemoveAsync(142, CancellationToken.None));
    }

    [Fact]
    public async Task RemoveAsync_WhenCoilAlreadyRemoved_ThrowsAlreadyRemovedException()
    {
        var removedAt = new DateTimeOffset(2026, 1, 10, 0, 0, 0, TimeSpan.Zero);
        var service = new CoilService(new MockCoilRepository
        {
            MarkRemovedAsyncMock = (_, _, _) => Task.FromResult<Coil?>(null),
            GetAsyncMock = (_, _) => Task.FromResult<IReadOnlyCollection<Coil>>(
                new[] { new Coil(142, 100, 200, removedAt.AddDays(-1), removedAt) })
        });

        await Assert.ThrowsAsync<CoilAlreadyRemovedException>(() =>
            service.RemoveAsync(142, CancellationToken.None));
    }

    [Fact]
    public async Task GetAsync_PassesCombinedFilterToRepository()
    {
        CoilFilter? capturedFilter = null;
        var service = new CoilService(new MockCoilRepository
        {
            GetAsyncMock = (filter, _) =>
            {
                capturedFilter = filter;
                return Task.FromResult<IReadOnlyCollection<Coil>>(Array.Empty<Coil>());
            }
        });

        var requestedFilter = new CoilFilter
        {
            IdFrom = 10,
            IdTo = 20,
            WeightFrom = 100,
            WeightTo = 200,
            OnlyInStock = true
        };

        await service.GetAsync(requestedFilter, CancellationToken.None);

        Assert.NotNull(capturedFilter);
        Assert.Equal(10, capturedFilter.IdFrom);
        Assert.Equal(20, capturedFilter.IdTo);
        Assert.Equal(100, capturedFilter.WeightFrom);
        Assert.Equal(200, capturedFilter.WeightTo);
        Assert.True(capturedFilter.OnlyInStock);
    }

    [Fact]
    public async Task GetStatisticsAsync_CalculatesPeriodStatistics()
    {
        var from = new DateTimeOffset(2026, 1, 10, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2026, 1, 12, 23, 59, 59, TimeSpan.Zero);
        var coils = new[]
        {
            new Coil(1, 10, 100, from.AddDays(-1), from.AddHours(12)),
            new Coil(2, 20, 200, from.AddHours(10), from.AddDays(2).AddHours(9)),
            new Coil(3, 30, 300, from.AddDays(1), null)
        };

        var service = new CoilService(new MockCoilRepository
        {
            GetOverlappingPeriodAsyncMock = (_, _, _) =>
                Task.FromResult<IReadOnlyCollection<Coil>>(coils)
        });

        var statistics = await service.GetStatisticsAsync(from, to, CancellationToken.None);

        Assert.Equal(2, statistics.AddedCount);
        Assert.Equal(2, statistics.RemovedCount);
        Assert.Equal(20, statistics.AverageLength);
        Assert.Equal(200, statistics.AverageWeight);
        Assert.Equal(10, statistics.MinLength);
        Assert.Equal(30, statistics.MaxLength);
        Assert.Equal(100, statistics.MinWeight);
        Assert.Equal(300, statistics.MaxWeight);
        Assert.Equal(600, statistics.TotalWeight);
        Assert.Equal(36, statistics.MinStorageDurationHours);
        Assert.Equal(47, statistics.MaxStorageDurationHours);
        Assert.Equal(new DateOnly(2026, 1, 10), statistics.MinCoilsCountDate);
        Assert.Equal(new DateOnly(2026, 1, 10), statistics.MaxCoilsCountDate);
        Assert.Equal(new DateOnly(2026, 1, 10), statistics.MinTotalWeightDate);
        Assert.Equal(new DateOnly(2026, 1, 11), statistics.MaxTotalWeightDate);
    }

    [Fact]
    public async Task GetStatisticsAsync_WithInvalidPeriod_ThrowsArgumentException()
    {
        var service = new CoilService(new MockCoilRepository());

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.GetStatisticsAsync(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(-1), CancellationToken.None));
    }
}

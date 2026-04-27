using SeverstalWarehouse.Api.Domain;
using SeverstalWarehouse.Api.Exceptions;
using SeverstalWarehouse.Api.Repositories;

namespace SeverstalWarehouse.Api.Services;

public sealed class CoilService(ICoilRepository repository) : ICoilService
{
    public Task<Coil> AddAsync(decimal length, decimal weight, CancellationToken cancellationToken)
    {
        if (length <= 0)
        {
            throw new ArgumentException("Length must be greater than zero.", nameof(length));
        }

        if (weight <= 0)
        {
            throw new ArgumentException("Weight must be greater than zero.", nameof(weight));
        }

        return repository.AddAsync(length, weight, DateTimeOffset.UtcNow, cancellationToken);
    }

    public async Task<Coil> RemoveAsync(long id, CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            throw new ArgumentException("Id must be greater than zero.", nameof(id));
        }

        var removed = await repository.MarkRemovedAsync(id, DateTimeOffset.UtcNow, cancellationToken);
        if (removed is not null)
        {
            return removed;
        }

        var existingCoils = await repository.GetAsync(
            new CoilFilter
            {
                IdFrom = id,
                IdTo = id
            },
            cancellationToken);

        return existingCoils.Count == 0
            ? throw new CoilNotFoundException(id)
            : throw new CoilAlreadyRemovedException(id);
    }

    public Task<IReadOnlyCollection<Coil>> GetAsync(CoilFilter filter, CancellationToken cancellationToken)
    {
        ValidateFilter(filter);
        return repository.GetAsync(NormalizeFilterDates(filter), cancellationToken);
    }

    public async Task<CoilStatistics> GetStatisticsAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken)
    {
        if (from > to)
        {
            throw new ArgumentException("Period start must be earlier than or equal to period end.", nameof(from));
        }

        var coils = await repository.GetOverlappingPeriodAsync(
            from.ToUniversalTime(),
            to.ToUniversalTime(),
            cancellationToken);
        var addedCount = coils.Count(coil => IsInRange(coil.AddedAt, from, to));
        var removedCount = coils.Count(coil => coil.RemovedAt is not null && IsInRange(coil.RemovedAt.Value, from, to));
        var removedInPeriod = coils
            .Where(coil => coil.RemovedAt is not null && IsInRange(coil.RemovedAt.Value, from, to))
            .ToArray();

        var durationHours = removedInPeriod
            .Select(coil => (coil.RemovedAt!.Value - coil.AddedAt).TotalHours)
            .ToArray();

        var dailySnapshots = BuildDailySnapshots(coils, from, to);

        return new CoilStatistics(
            from,
            to,
            addedCount,
            removedCount,
            coils.Count == 0 ? null : coils.Average(coil => coil.Length),
            coils.Count == 0 ? null : coils.Average(coil => coil.Weight),
            coils.Count == 0 ? null : coils.Min(coil => coil.Length),
            coils.Count == 0 ? null : coils.Max(coil => coil.Length),
            coils.Count == 0 ? null : coils.Min(coil => coil.Weight),
            coils.Count == 0 ? null : coils.Max(coil => coil.Weight),
            coils.Sum(coil => coil.Weight),
            durationHours.Length == 0 ? null : durationHours.Min(),
            durationHours.Length == 0 ? null : durationHours.Max(),
            dailySnapshots.MinBy(snapshot => snapshot.Count)?.Date,
            dailySnapshots.MaxBy(snapshot => snapshot.Count)?.Date,
            dailySnapshots.MinBy(snapshot => snapshot.TotalWeight)?.Date,
            dailySnapshots.MaxBy(snapshot => snapshot.TotalWeight)?.Date);
    }

    private static void ValidateFilter(CoilFilter filter)
    {
        ValidateRange(filter.IdFrom, filter.IdTo, "id");
        ValidateRange(filter.WeightFrom, filter.WeightTo, "weight");
        ValidateRange(filter.LengthFrom, filter.LengthTo, "length");
        ValidateRange(filter.AddedFrom, filter.AddedTo, "added date");
        ValidateRange(filter.RemovedFrom, filter.RemovedTo, "removed date");
    }

    private static CoilFilter NormalizeFilterDates(CoilFilter filter) =>
        new()
        {
            IdFrom = filter.IdFrom,
            IdTo = filter.IdTo,
            WeightFrom = filter.WeightFrom,
            WeightTo = filter.WeightTo,
            LengthFrom = filter.LengthFrom,
            LengthTo = filter.LengthTo,
            AddedFrom = filter.AddedFrom?.ToUniversalTime(),
            AddedTo = filter.AddedTo?.ToUniversalTime(),
            RemovedFrom = filter.RemovedFrom?.ToUniversalTime(),
            RemovedTo = filter.RemovedTo?.ToUniversalTime(),
            OnlyInStock = filter.OnlyInStock
        };

    private static void ValidateRange<T>(T? from, T? to, string rangeName)
        where T : struct, IComparable<T>
    {
        if (from is not null && to is not null && from.Value.CompareTo(to.Value) > 0)
        {
            throw new ArgumentException($"Invalid {rangeName} range: start must be less than or equal to end.");
        }
    }

    private static bool IsInRange(DateTimeOffset value, DateTimeOffset from, DateTimeOffset to) =>
        value >= from && value <= to;

    private static IReadOnlyCollection<DailySnapshot> BuildDailySnapshots(
        IReadOnlyCollection<Coil> coils,
        DateTimeOffset from,
        DateTimeOffset to)
    {
        var snapshots = new List<DailySnapshot>();
        for (var date = DateOnly.FromDateTime(from.DateTime);
             date <= DateOnly.FromDateTime(to.DateTime);
             date = date.AddDays(1))
        {
            var dayStart = new DateTimeOffset(date.ToDateTime(TimeOnly.MinValue), from.Offset);
            var dayEnd = new DateTimeOffset(date.ToDateTime(TimeOnly.MaxValue), from.Offset);
            var coilsOnDay = coils
                .Where(coil => coil.AddedAt <= dayEnd && (coil.RemovedAt is null || coil.RemovedAt >= dayStart))
                .ToArray();

            snapshots.Add(new DailySnapshot(date, coilsOnDay.Length, coilsOnDay.Sum(coil => coil.Weight)));
        }

        return snapshots;
    }

    private sealed record DailySnapshot(DateOnly Date, int Count, decimal TotalWeight);
}

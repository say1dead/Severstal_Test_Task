using System.Data;
using Npgsql;
using SeverstalWarehouse.Api.Domain;

namespace SeverstalWarehouse.Api.Repositories;

public sealed class PostgresCoilRepository(NpgsqlDataSource dataSource) : ICoilRepository
{
    private static readonly SemaphoreSlim SchemaLock = new(1, 1);
    private static bool _schemaReady;

    public async Task<Coil> AddAsync(
        decimal length,
        decimal weight,
        DateTimeOffset addedAt,
        CancellationToken cancellationToken)
    {
        await EnsureSchemaAsync(cancellationToken);

        const string sql = """
            INSERT INTO coils (length, weight, added_at)
            VALUES (@length, @weight, @addedAt)
            RETURNING id, length, weight, added_at, removed_at;
            """;

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("length", length);
        command.Parameters.AddWithValue("weight", weight);
        command.Parameters.AddWithValue("addedAt", addedAt);

        await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken);
        await reader.ReadAsync(cancellationToken);

        return ReadCoil(reader);
    }

    public async Task<Coil?> MarkRemovedAsync(long id, DateTimeOffset removedAt, CancellationToken cancellationToken)
    {
        await EnsureSchemaAsync(cancellationToken);

        const string sql = """
            UPDATE coils
            SET removed_at = @removedAt
            WHERE id = @id AND removed_at IS NULL
            RETURNING id, length, weight, added_at, removed_at;
            """;

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", id);
        command.Parameters.AddWithValue("removedAt", removedAt);

        await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return ReadCoil(reader);
    }

    public async Task<IReadOnlyCollection<Coil>> GetAsync(CoilFilter filter, CancellationToken cancellationToken)
    {
        await EnsureSchemaAsync(cancellationToken);

        var conditions = new List<string>();
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();

        AddRangeCondition(command, conditions, "id", "idFrom", ">=", filter.IdFrom);
        AddRangeCondition(command, conditions, "id", "idTo", "<=", filter.IdTo);
        AddRangeCondition(command, conditions, "weight", "weightFrom", ">=", filter.WeightFrom);
        AddRangeCondition(command, conditions, "weight", "weightTo", "<=", filter.WeightTo);
        AddRangeCondition(command, conditions, "length", "lengthFrom", ">=", filter.LengthFrom);
        AddRangeCondition(command, conditions, "length", "lengthTo", "<=", filter.LengthTo);
        AddRangeCondition(command, conditions, "added_at", "addedFrom", ">=", filter.AddedFrom);
        AddRangeCondition(command, conditions, "added_at", "addedTo", "<=", filter.AddedTo);
        AddRangeCondition(command, conditions, "removed_at", "removedFrom", ">=", filter.RemovedFrom);
        AddRangeCondition(command, conditions, "removed_at", "removedTo", "<=", filter.RemovedTo);

        if (filter.OnlyInStock is true)
        {
            conditions.Add("removed_at IS NULL");
        }
        else if (filter.OnlyInStock is false)
        {
            conditions.Add("removed_at IS NOT NULL");
        }

        command.CommandText = $"""
            SELECT id, length, weight, added_at, removed_at
            FROM coils
            {(conditions.Count == 0 ? string.Empty : "WHERE " + string.Join(" AND ", conditions))}
            ORDER BY id;
            """;

        return await ReadCoilsAsync(command, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Coil>> GetOverlappingPeriodAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken)
    {
        await EnsureSchemaAsync(cancellationToken);

        const string sql = """
            SELECT id, length, weight, added_at, removed_at
            FROM coils
            WHERE added_at <= @to
              AND (removed_at IS NULL OR removed_at >= @from)
            ORDER BY id;
            """;

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("from", from);
        command.Parameters.AddWithValue("to", to);

        return await ReadCoilsAsync(command, cancellationToken);
    }

    private async Task EnsureSchemaAsync(CancellationToken cancellationToken)
    {
        if (_schemaReady)
        {
            return;
        }

        await SchemaLock.WaitAsync(cancellationToken);
        try
        {
            if (_schemaReady)
            {
                return;
            }

            const string sql = """
                CREATE TABLE IF NOT EXISTS coils (
                    id BIGINT GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    length NUMERIC(18, 3) NOT NULL CHECK (length > 0),
                    weight NUMERIC(18, 3) NOT NULL CHECK (weight > 0),
                    added_at TIMESTAMPTZ NOT NULL,
                    removed_at TIMESTAMPTZ NULL
                );

                CREATE INDEX IF NOT EXISTS ix_coils_weight ON coils(weight);
                CREATE INDEX IF NOT EXISTS ix_coils_length ON coils(length);
                CREATE INDEX IF NOT EXISTS ix_coils_added_at ON coils(added_at);
                CREATE INDEX IF NOT EXISTS ix_coils_removed_at ON coils(removed_at);
                """;

            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
            await using var command = new NpgsqlCommand(sql, connection);
            await command.ExecuteNonQueryAsync(cancellationToken);
            _schemaReady = true;
        }
        finally
        {
            SchemaLock.Release();
        }
    }

    private static void AddRangeCondition<T>(
        NpgsqlCommand command,
        ICollection<string> conditions,
        string columnName,
        string parameterName,
        string operation,
        T? value)
        where T : struct
    {
        if (value is null)
        {
            return;
        }

        conditions.Add($"{columnName} {operation} @{parameterName}");
        command.Parameters.AddWithValue(parameterName, value.Value);
    }

    private static async Task<IReadOnlyCollection<Coil>> ReadCoilsAsync(
        NpgsqlCommand command,
        CancellationToken cancellationToken)
    {
        var coils = new List<Coil>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            coils.Add(ReadCoil(reader));
        }

        return coils;
    }

    private static Coil ReadCoil(NpgsqlDataReader reader) =>
        new(
            reader.GetInt64(0),
            reader.GetDecimal(1),
            reader.GetDecimal(2),
            reader.GetFieldValue<DateTimeOffset>(3),
            reader.IsDBNull(4) ? null : reader.GetFieldValue<DateTimeOffset>(4));
}

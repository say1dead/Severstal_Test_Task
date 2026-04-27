namespace SeverstalWarehouse.Api.Exceptions;

public sealed class CoilAlreadyRemovedException(long id)
    : Exception($"Coil with id '{id}' has already been removed from the warehouse.");


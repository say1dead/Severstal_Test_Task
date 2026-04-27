namespace SeverstalWarehouse.Api.Exceptions;

public sealed class CoilNotFoundException(long id)
    : Exception($"Coil with id '{id}' was not found on the warehouse.");


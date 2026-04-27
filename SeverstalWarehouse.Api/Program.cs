using Microsoft.AspNetCore.Diagnostics;
using Npgsql;
using SeverstalWarehouse.Api.Domain;
using SeverstalWarehouse.Api.Exceptions;
using SeverstalWarehouse.Api.Repositories;
using SeverstalWarehouse.Api.Services;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

builder.Services.AddProblemDetails();

var connectionString = builder.Configuration.GetConnectionString("WarehouseDb")
    ?? throw new InvalidOperationException("Connection string 'WarehouseDb' is not configured.");

builder.Services.AddSingleton(NpgsqlDataSource.Create(connectionString));
builder.Services.AddScoped<ICoilRepository, PostgresCoilRepository>();
builder.Services.AddScoped<ICoilService, CoilService>();

var app = builder.Build();

app.MapOpenApi();
app.UseSwagger();
app.UseSwaggerUI();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        var (statusCode, title, detail) = exception switch
        {
            CoilNotFoundException => (StatusCodes.Status404NotFound, "Coil not found", exception.Message),
            CoilAlreadyRemovedException => (StatusCodes.Status409Conflict, "Coil already removed", exception.Message),
            ArgumentException => (StatusCodes.Status400BadRequest, "Invalid request", exception.Message),
            PostgresException => (StatusCodes.Status503ServiceUnavailable, "Database error", "PostgreSQL rejected the operation."),
            NpgsqlException => (StatusCodes.Status503ServiceUnavailable, "Database unavailable", "PostgreSQL is unavailable."),
            _ => (StatusCodes.Status500InternalServerError, "Unexpected error", "An unexpected error occurred.")
        };

        context.Response.StatusCode = statusCode;
        await Results.Problem(detail, statusCode: statusCode, title: title).ExecuteAsync(context);
    });
});

if (!app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.MapControllers();

app.Run();

using Microsoft.AspNetCore.Mvc;
using SeverstalWarehouse.Api.Contracts;
using SeverstalWarehouse.Api.Domain;
using SeverstalWarehouse.Api.Exceptions;
using SeverstalWarehouse.Api.Services;

namespace SeverstalWarehouse.Api.Controllers;

[ApiController]
[Route("api/coils")]
public sealed class CoilsController(ICoilService coilService) : ControllerBase
{
    /// <summary>
    /// enpdoint for add coil
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(typeof(Coil), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<Coil>> AddAsync(
        CreateCoilRequest request,
        CancellationToken cancellationToken)
    {
        var coil = await coilService.AddAsync(request.Length, request.Weight, cancellationToken);
        return Created($"/api/coils/{coil.Id}", coil);
    }

    /// <summary>
    /// enpdoint for remove coil
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(typeof(Coil), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<Coil>> RemoveAsync(long id, CancellationToken cancellationToken)
    {
        var coil = await coilService.RemoveAsync(id, cancellationToken);
        return Ok(coil);
    }

    /// <summary>
    /// enpdoint for get coil by id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="CoilNotFoundException"></exception>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(Coil), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<Coil>> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        var coils = await coilService.GetAsync(
            new CoilFilter
            {
                IdFrom = id,
                IdTo = id
            },
            cancellationToken);

        var coil = coils.SingleOrDefault();
        if (coil is null)
        {
            throw new CoilNotFoundException(id);
        }

        return Ok(coil);
    }

    /// <summary>
    /// enpdoint for get coils
    /// </summary>
    /// <param name="query"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<Coil>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<IReadOnlyCollection<Coil>>> GetAsync(
        [FromQuery] CoilQueryParameters query,
        CancellationToken cancellationToken)
    {
        var filter = new CoilFilter
        {
            IdFrom = query.IdFrom,
            IdTo = query.IdTo,
            WeightFrom = query.WeightFrom,
            WeightTo = query.WeightTo,
            LengthFrom = query.LengthFrom,
            LengthTo = query.LengthTo,
            AddedFrom = query.AddedFrom,
            AddedTo = query.AddedTo,
            RemovedFrom = query.RemovedFrom,
            RemovedTo = query.RemovedTo,
            OnlyInStock = query.OnlyInStock
        };

        var coils = await coilService.GetAsync(filter, cancellationToken);
        return Ok(coils);
    }

    /// <summary>
    /// enpdoint for get statistics
    /// </summary>
    /// <param name="query"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(CoilStatistics), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<CoilStatistics>> GetStatisticsAsync(
        [FromQuery] StatisticsQueryParameters query,
        CancellationToken cancellationToken)
    {
        if (query.From is null || query.To is null)
        {
            return BadRequest("Query parameters 'from' and 'to' are required.");
        }

        var statistics = await coilService.GetStatisticsAsync(query.From.Value, query.To.Value, cancellationToken);
        return Ok(statistics);
    }
}

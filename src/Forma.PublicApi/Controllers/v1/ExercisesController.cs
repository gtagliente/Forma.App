using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Mime;
using System.Threading.Tasks;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Forma.Application.Exercise.Commands;
using Forma.Application.Exercise.Responses;
using Forma.PublicApi.Extensions;
using Forma.PublicApi.Models;
using Forma.Query.Application.Exercise.Queries;
using Forma.Query.QueriesModel;
using Forma.PublicApi.Filters.Exceptions;

namespace Forma.PublicApi.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/[controller]")]
[TypeFilter<DomainExceptionToActionResultFilter>]

public class ExercisesController(IMediator mediator) : ControllerBase
{
    ////////////////////////
    // POST: /api/exercises
    ////////////////////////

    /// <summary>
    /// Register a new customer.
    /// </summary>
    /// <response code="201">Returns the Id of the new exercise.</response>
    /// <response code="400">Returns list of errors if the request is invalid.</response>
    /// <response code="500">When an unexpected internal error occurs on the server.</response>
    [HttpPost]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ApiResponse<CreatedExerciseResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody][Required] CreateExerciseCommand command) =>
        (await mediator.Send(command)).ToActionResult();

    ///////////////////////
    // PUT: /api/customers
    //////////////////////

    ///// <summary>
    ///// Updates an existing client.
    ///// </summary>
    ///// <response code="200">Returns the response with the success message.</response>
    ///// <response code="400">Returns list of errors if the request is invalid.</response>
    ///// <response code="404">When no client is found by the given Id.</response>
    ///// <response code="500">When an unexpected internal error occurs on the server.</response>
    //[HttpPut]
    //[Consumes(MediaTypeNames.Application.Json)]
    //[Produces(MediaTypeNames.Application.Json)]
    //[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    //[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    //[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    //[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    //public async Task<IActionResult> Update([FromBody][Required] UpdateCustomerCommand command) =>
    //    (await mediator.Send(command)).ToActionResult();

    //////////////////////////////
    //// DELETE: /api/customers/{id}
    ////////////////////////////////

    ///// <summary>
    ///// Deletes the client by Id.
    ///// </summary>
    ///// <response code="200">Returns the response with the success message.</response>
    ///// <response code="400">Returns list of errors if the request is invalid.</response>
    ///// <response code="404">When no client is found by the given Id.</response>
    ///// <response code="500">When an unexpected internal error occurs on the server.</response>
    //[HttpDelete("{id:guid}")]
    //[Consumes(MediaTypeNames.Application.Json)]
    //[Produces(MediaTypeNames.Application.Json)]
    //[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    //[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    //[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    //[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    //public async Task<IActionResult> Delete([Required] Guid id) =>
    //    (await mediator.Send(new DeleteCustomerCommand(id))).ToActionResult();

    ///////////////////////////
    // GET: /api/customers/{id}
    ///////////////////////////

    ///// <summary>
    ///// Gets the client by Id.
    ///// </summary>
    ///// <response code="200">Returns the client.</response>
    ///// <response code="400">Returns list of errors if the request is invalid.</response>
    ///// <response code="404">When no client is found by the given Id.</response>
    ///// <response code="500">When an unexpected internal error occurs on the server.</response>
    //[HttpGet("{id:guid}")]
    //[Consumes(MediaTypeNames.Application.Json)]
    //[Produces(MediaTypeNames.Application.Json)]
    //[ProducesResponseType(typeof(ApiResponse<CustomerQueryModel>), StatusCodes.Status200OK)]
    //[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    //[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    //[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    //public async Task<IActionResult> GetById([Required] Guid id) =>
    //    (await mediator.Send(new GetCustomerByIdQuery(id))).ToActionResult();

    //////////////////////
    // GET: /api/customers
    //////////////////////

    /// <summary>
    /// Gets a list of all customers.
    /// </summary>
    /// <response code="200">Returns the list of clients.</response>
    /// <response code="500">When an unexpected internal error occurs on the server.</response>
    [HttpGet]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ExerciseQueryModel>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll() =>
        (await mediator.Send(new GetAllExerciseQuery())).ToActionResult();
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Mime;
using System.Threading.Tasks;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Forma.Application.Exercise.Commands;
using Forma.Application.Exercise.Responses;
using Forma.CoreInfrastructure.Abstractions;
using Forma.Domain.Entities.ExerciseAggregate;
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

public class ExercisesController(IMediator mediator, ICurrentUserAccessor currentUserAccessor) : ControllerBase
{
    ////////////////////////
    // POST: /api/exercises
    ////////////////////////

    /// <summary>
    /// Register a new customer.
    /// </summary>
    /// <response code="201">Returns the Id of the new exercise.</response>
    /// <response code="400">Returns list of errors if the request is invalid.</response>
    /// <response code="401">When no valid bearer token is supplied.</response>
    /// <response code="500">When an unexpected internal error occurs on the server.</response>
    [Authorize]
    [HttpPost(nameof(Create))]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ApiResponse<CreatedExerciseResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody][Required] CreateExerciseCommand command)
    {
        // OwnerId is never bound from the request (see CreateExerciseCommand.OwnerId's
        // [JsonIgnore]) — the acting user's id always comes from the validated token, never a
        // caller-asserted value. Shared vs. private is signalled only by the non-identity-bearing
        // "Shared" flag. See ADR-007-jwt-bearer-authentication.md.
        command.OwnerId = command.Shared ? null : currentUserAccessor.UserId;
        return (await mediator.Send(command)).ToActionResult();
    }

    /////////////////////////////
    // PUT: /api/exercises/Update
    /////////////////////////////

    /// <summary>
    /// Updates an existing exercise's name, description, and/or muscle groups.
    /// </summary>
    /// <response code="200">Returns the response with the success message.</response>
    /// <response code="400">Returns list of errors if the request is invalid.</response>
    /// <response code="401">When no valid bearer token is supplied.</response>
    /// <response code="403">When the exercise is privately owned by another user.</response>
    /// <response code="404">When no exercise is found by the given Id.</response>
    /// <response code="500">When an unexpected internal error occurs on the server.</response>
    [Authorize]
    [HttpPut(nameof(Update))]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update([FromBody][Required] UpdateExerciseCommand command) =>
        (await mediator.Send(command)).ToActionResult();

    //////////////////////////////////
    // DELETE: /api/exercises/{id:guid}
    //////////////////////////////////

    /// <summary>
    /// Deletes an exercise by Id. Fails if the exercise still has children in the hierarchy.
    /// </summary>
    /// <response code="200">Returns the response with the success message.</response>
    /// <response code="400">Returns list of errors if the request is invalid.</response>
    /// <response code="401">When no valid bearer token is supplied.</response>
    /// <response code="403">When the exercise is privately owned by another user.</response>
    /// <response code="404">When no exercise is found by the given Id.</response>
    /// <response code="500">When an unexpected internal error occurs on the server.</response>
    [Authorize]
    [HttpDelete("{id:guid}")]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete([Required] Guid id) =>
        (await mediator.Send(new DeleteExerciseCommand(new ExerciseId(id)))).ToActionResult();

    ///////////////////////////
    // GET: /api/exercises/{id}
    ///////////////////////////

    /// <summary>
    /// Gets an Exercise by Id. Also backs ADR-006 Rule 1 (Exercise-existence check at Workout
    /// create/edit) once consumed by training-planning-service.
    /// </summary>
    /// <response code="200">Returns the exercise.</response>
    /// <response code="400">Returns list of errors if the request is invalid.</response>
    /// <response code="404">When no exercise is found by the given Id.</response>
    /// <response code="500">When an unexpected internal error occurs on the server.</response>
    [HttpGet("{id:guid}")]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ApiResponse<ExerciseQueryModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById([Required] Guid id) =>
        (await mediator.Send(new GetExerciseByIdQuery(id))).ToActionResult();

    //////////////////////
    // GET: /api/customers
    //////////////////////

    /// <summary>
    /// Gets a list of all customers.
    /// </summary>
    /// <response code="200">Returns the list of clients.</response>
    /// <response code="500">When an unexpected internal error occurs on the server.</response>
    [HttpGet(nameof(GetAll))]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ExerciseQueryModel>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll() =>
        // Anonymous-accessible (no [Authorize]) — shared-library browsing shouldn't require a
        // token. An anonymous caller's UserId is null, which GetVisibleToAsync's filter
        // (OwnerId == null || OwnerId == requestingUserId) naturally resolves to shared-only.
        (await mediator.Send(new GetAllExerciseQuery(currentUserAccessor.UserId))).ToActionResult();


    /// <summary>
    /// Register a new customer.
    /// </summary>
    /// <response code="201">Returns the Id of the new exercise resource.</response>
    /// <response code="400">Returns list of errors if the request is invalid.</response>
    /// <response code="401">When no valid bearer token is supplied.</response>
    /// <response code="500">When an unexpected internal error occurs on the server.</response>
    [Authorize]
    [HttpPost(nameof(CreateExerciseResource))]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ApiResponse<CreatedExerciseResourceResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateExerciseResource([FromBody][Required] CreateExerciseResourceCommand command) =>
        (await mediator.Send(command)).ToActionResult();

    ///////////////////////////////
    // POST: /api/exercises/SetParent
    ///////////////////////////////

    /// <summary>
    /// Sets (or changes) an Exercise's parent, forming a generalization/specialization relationship.
    /// </summary>
    /// <response code="200">Returns the response with the success message.</response>
    /// <response code="400">Returns list of errors if the request is invalid.</response>
    /// <response code="401">When no valid bearer token is supplied.</response>
    /// <response code="404">When no exercise is found by the given Id.</response>
    /// <response code="500">When an unexpected internal error occurs on the server.</response>
    [Authorize]
    [HttpPost(nameof(SetParent))]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SetParent([FromBody][Required] SetExerciseParentCommand command) =>
        (await mediator.Send(command)).ToActionResult();

    /////////////////////////////////
    // POST: /api/exercises/ClearParent
    /////////////////////////////////

    /// <summary>
    /// Clears an Exercise's parent, if any.
    /// </summary>
    /// <response code="200">Returns the response with the success message.</response>
    /// <response code="400">Returns list of errors if the request is invalid.</response>
    /// <response code="401">When no valid bearer token is supplied.</response>
    /// <response code="404">When no exercise is found by the given Id.</response>
    /// <response code="500">When an unexpected internal error occurs on the server.</response>
    [Authorize]
    [HttpPost(nameof(ClearParent))]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ClearParent([FromBody][Required] ClearExerciseParentCommand command) =>
        (await mediator.Send(command)).ToActionResult();
}
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Ardalis.Result.FluentValidation;
using FluentValidation;
using MediatR;
using Forma.Application.Exercise.Commands;
using Forma.Application.Exercise.Responses;
using Forma.CoreInfrastructure.Abstractions;
using Forma.CoreInfrastructure.Caching;
using DOMAIN_ENTITIES = Forma.Domain.Entities;
using Forma.Domain.Builders.Contracts;
using System;
using Forma.Domain.Entities.ExerciseAggregate;


namespace Forma.Application.Exercise.Handlers;

public class CreateExerciseCommandHandler(
    IValidator<CreateExerciseCommand> validator,
    IExerciseWriteOnlyRepository<DOMAIN_ENTITIES.ExerciseAggregate.Exercise, ExerciseId> repository,
    IExerciseBuilder builder,
    IUnitOfWork unitOfWork,
    ICacheService cacheService,
    ICurrentUserAccessor currentUserAccessor) : IRequestHandler<CreateExerciseCommand, Result<CreatedExerciseResponse>>
{
    public async Task<Result<CreatedExerciseResponse>> Handle(
        CreateExerciseCommand request,
        CancellationToken cancellationToken)
    {
        // Validating the request.
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            // Return the result with validation errors.
            return Result<CreatedExerciseResponse>.Invalid(validationResult.AsErrors());
        }


        // Creating an instance of the exercise entity.
        // When instantiated, the "ExerciseCreatedEvent" will be created.
        var exercise = await DOMAIN_ENTITIES.ExerciseAggregate.Exercise.Create(builder, request.Name, request.MuscleGroups, request.Description, request.OwnerId, request.ParentId);

        // Adding the entity to the repository.
        repository.Add(exercise);

        // Saving changes to the database and triggering events.
        await unitOfWork.SaveChangesAsync();

        // Load-bearing cache fix: the event-driven invalidation only knows the Exercise's
        // OwnerId (null for shared Exercises), not who acted — the command handler always knows
        // the real actor.
        await cacheService.RemoveAsync(ExerciseCacheKeys.ForUser(currentUserAccessor.UserId));

        // Returning the ID.
        return Result<CreatedExerciseResponse>.Created(
            new CreatedExerciseResponse(exercise.Id.Value), location: $"/api/exercises/{exercise.Id}");
    }
}
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Ardalis.Result.FluentValidation;
using FluentValidation;
using MediatR;
using Forma.Application.Exercise.Commands;
using Forma.Application.Exercise.Responses;
using Forma.CoreInfrastructure.Abstractions;
using DOMAIN_ENTITIES = Forma.Domain.Entities;
using Forma.Domain.Builders.Contracts;
using System;
using Forma.Domain.Entities.ExerciseAggregate;


namespace Forma.Application.Exercise.Handlers;

//internal class CreateExerciseResourceCommandHandler
//{
//}

public class CreateExerciseResourceCommandHandler(
    IValidator<CreateExerciseResourceCommand> validator,
    IExerciseWriteOnlyRepository<DOMAIN_ENTITIES.ExerciseAggregate.Exercise, ExerciseId> repository,
    IExerciseResourceWriteOnlyRepository<ExerciseResource, ExerciseResourceId> exerciseResourceRepository,
    IExerciseBuilder builder,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateExerciseResourceCommand, Result<CreatedExerciseResourceResponse>>
{
    public async Task<Result<CreatedExerciseResourceResponse>> Handle(
        CreateExerciseResourceCommand request,
        CancellationToken cancellationToken)
    {
        // Validating the request.
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            // Return the result with validation errors.
            return Result<CreatedExerciseResourceResponse>.Invalid(validationResult.AsErrors());
        }

        // Creating an instance of the exercise entity.
        // When instantiated, the "ExerciseCreatedEvent" will be created.
        var exercise = await repository.GetByIdAsync(request.ExerciseId);
        if(exercise == null)
            return Result<CreatedExerciseResourceResponse>.NotFound($"Exercise with Id {request.ExerciseId} not found");

        var exerciseResource = await exercise.AddResource(
            builder,
            request.Title,
            request.Content,
            request.Type,
            request.Link);

        // exercise was loaded untracked (GetByIdAsync uses AsNoTrackingWithIdentityResolution),
        // and exerciseResource has a client-generated key already set, so repository.Update(exercise)
        // would have EF classify it as Modified rather than Added — a DbUpdateConcurrencyException
        // against a row that doesn't exist yet. Track the new resource explicitly instead.
        exerciseResourceRepository.Add(exerciseResource);

        //Saving changes to the database and triggering events.
        await unitOfWork.SaveChangesAsync();

        // Returning the ID.
        return Result<CreatedExerciseResourceResponse>.Created(
            new CreatedExerciseResourceResponse(exerciseResource.Id.Value), location: $"/api/exercises/resource/{exerciseResource.Id.Value}");
    }
}
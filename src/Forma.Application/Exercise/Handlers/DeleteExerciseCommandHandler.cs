using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Ardalis.Result.FluentValidation;
using FluentValidation;
using MediatR;
using Forma.Application.Exercise.Commands;
using Forma.CoreInfrastructure.Abstractions;
using DOMAIN_ENTITIES = Forma.Domain.Entities;
using Forma.Domain.Builders.Contracts;
using Forma.Domain.Entities.ExerciseAggregate;

namespace Forma.Application.Exercise.Handlers;

public class DeleteExerciseCommandHandler(
    IValidator<DeleteExerciseCommand> validator,
    IExerciseWriteOnlyRepository<DOMAIN_ENTITIES.ExerciseAggregate.Exercise, ExerciseId> repository,
    IExerciseBuilder builder,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteExerciseCommand, Result>
{
    public async Task<Result> Handle(DeleteExerciseCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return Result.Invalid(validationResult.AsErrors());

        var exercise = await repository.GetByIdAsync(request.ExerciseId);
        if (exercise == null)
            return Result.NotFound($"Exercise with Id {request.ExerciseId} not found");

        // Raises ExerciseDeletedEvent and throws if the exercise still has children in the hierarchy.
        await exercise.Delete(builder);

        repository.Remove(exercise);
        await unitOfWork.SaveChangesAsync();

        return Result.SuccessWithMessage("Successfully removed!");
    }
}

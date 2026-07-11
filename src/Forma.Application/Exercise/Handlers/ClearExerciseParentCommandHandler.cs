using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Ardalis.Result.FluentValidation;
using FluentValidation;
using MediatR;
using Forma.Application.Exercise.Commands;
using Forma.CoreInfrastructure.Abstractions;
using DOMAIN_ENTITIES = Forma.Domain.Entities;
using Forma.Domain.Entities.ExerciseAggregate;

namespace Forma.Application.Exercise.Handlers;

public class ClearExerciseParentCommandHandler(
    IValidator<ClearExerciseParentCommand> validator,
    IExerciseWriteOnlyRepository<DOMAIN_ENTITIES.ExerciseAggregate.Exercise, ExerciseId> repository,
    IUnitOfWork unitOfWork) : IRequestHandler<ClearExerciseParentCommand, Result>
{
    public async Task<Result> Handle(ClearExerciseParentCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return Result.Invalid(validationResult.AsErrors());

        var exercise = await repository.GetByIdAsync(request.ExerciseId);
        if (exercise == null)
            return Result.NotFound($"Exercise with Id {request.ExerciseId} not found");

        exercise.ClearParent();

        repository.Update(exercise);
        await unitOfWork.SaveChangesAsync();

        return Result.SuccessWithMessage("Parent cleared successfully!");
    }
}

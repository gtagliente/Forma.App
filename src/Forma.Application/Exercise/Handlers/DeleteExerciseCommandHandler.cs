using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Ardalis.Result.FluentValidation;
using FluentValidation;
using MediatR;
using Forma.Application.Exercise.Commands;
using Forma.CoreInfrastructure.Abstractions;
using Forma.CoreInfrastructure.Caching;
using DOMAIN_ENTITIES = Forma.Domain.Entities;
using Forma.Domain.Builders.Contracts;
using Forma.Domain.Entities.ExerciseAggregate;

namespace Forma.Application.Exercise.Handlers;

public class DeleteExerciseCommandHandler(
    IValidator<DeleteExerciseCommand> validator,
    IExerciseWriteOnlyRepository<DOMAIN_ENTITIES.ExerciseAggregate.Exercise, ExerciseId> repository,
    IExerciseBuilder builder,
    IUnitOfWork unitOfWork,
    ICacheService cacheService,
    ICurrentUserAccessor currentUserAccessor) : IRequestHandler<DeleteExerciseCommand, Result>
{
    public async Task<Result> Handle(DeleteExerciseCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return Result.Invalid(validationResult.AsErrors());

        var exercise = await repository.GetByIdAsync(request.ExerciseId);
        if (exercise == null)
            return Result.NotFound($"Exercise with Id {request.ExerciseId} not found");

        // Shared Exercises (OwnerId == null) may be deleted by any authenticated user — no
        // curator/role concept exists yet. A private Exercise may only be deleted by its owner.
        // See ADR-007-jwt-bearer-authentication.md.
        if (exercise.OwnerId is not null && exercise.OwnerId != currentUserAccessor.UserId)
            return Result.Forbidden();

        // Raises ExerciseDeletedEvent and throws if the exercise still has children in the hierarchy.
        await exercise.Delete(builder);

        repository.Remove(exercise);
        await unitOfWork.SaveChangesAsync();

        // Load-bearing cache fix: the event-driven invalidation only knows the Exercise's
        // OwnerId (null for shared Exercises), not who acted — the command handler always knows
        // the real actor.
        await cacheService.RemoveAsync(ExerciseCacheKeys.ForUser(currentUserAccessor.UserId));

        return Result.SuccessWithMessage("Successfully removed!");
    }
}

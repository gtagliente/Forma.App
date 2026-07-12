using System;
using System.Threading.Tasks;
using Forma.Domain.Entities.ExerciseAggregate;
using Forma.Domain.Entities.ExerciseAggregate.Contracts;
using Microsoft.Extensions.Logging;

namespace Forma.Infrastructure.ExternalServices.TrainingPlanning;

/// <summary>
/// ADR-006 Rule 2 adapter: wraps the Kiota-generated <see cref="TrainingPlanningApiClient"/>.
/// Fails closed by construction — a timeout (via the resilience handler registered in
/// ConfigureServices), connection failure, or unexpected/empty response is treated as
/// "referenced," never as "not referenced." See
/// Forma.Claude/docs/architecture/integration-patterns.md, "Security note" for the
/// still-unauthenticated state of this call, and ADR-006 for the fail-closed rationale.
/// </summary>
internal class ExerciseUsageChecker(TrainingPlanningApiClient client, ILogger<ExerciseUsageChecker> logger)
    : IExerciseUsageChecker
{
    public async Task<bool> IsReferencedByAnyWorkoutAsync(ExerciseId id)
    {
        try
        {
            var response = await client.Api.Internal.Exercisereferences.Isreferenced.GetAsync(
                config => config.QueryParameters.ExerciseId = id.Value);

            // A missing/null Result is itself inconclusive — fail closed, don't assume "not referenced".
            return response?.Result ?? true;
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "Could not confirm with training-planning-service whether Exercise {ExerciseId} is referenced by any Workout; blocking the delete (fail-closed, ADR-006 Rule 2).",
                id);
            return true;
        }
    }
}

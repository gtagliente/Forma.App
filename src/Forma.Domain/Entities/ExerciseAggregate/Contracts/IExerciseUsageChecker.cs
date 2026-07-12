using System.Threading.Tasks;

namespace Forma.Domain.Entities.ExerciseAggregate.Contracts;

/// <summary>
/// Cross-service check (ADR-006 Rule 2, Forma.Claude/docs/architecture/adr/ADR-006-cross-service-reference-integrity.md):
/// before deleting an Exercise, asks training-planning-service whether any Workout's current
/// version still references it. The implementation fails closed — an inconclusive answer
/// (timeout, unreachable) must be treated the same as "yes, referenced," never as "no."
/// </summary>
public interface IExerciseUsageChecker
{
    Task<bool> IsReferencedByAnyWorkoutAsync(ExerciseId id);
}

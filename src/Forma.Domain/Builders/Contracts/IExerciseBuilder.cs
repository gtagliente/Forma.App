using Forma.Domain.Entities.ExerciseAggregate;
using Forma.Domain.Entities.ExerciseAggregate.Contracts;

namespace Forma.Domain.Builders.Contracts;

public interface IExerciseBuilder
{
    Contracts _contracts { internal get; init; }

    struct Contracts
    {
        public readonly IExerciseUniquenessChecker uniquenessChecker {  get; init; }

        public  readonly IExerciseResourceLinkUniquenessChecker exerciseResourceLinkUniquenessChecker {  get; init; }

        public readonly IExerciseHierarchyChecker hierarchyChecker { get; init; }

        public readonly IExerciseUsageChecker usageChecker { get; init; }
    }

}

using Forma.Domain.Builders.Contracts;
using Forma.Domain.Entities.ExerciseAggregate.Contracts;

namespace Forma.Domain.Builders;
internal class ExerciseBuilder : IExerciseBuilder
{
    public IExerciseBuilder.Contracts _contracts {  get; init; }

    public ExerciseBuilder(IExerciseUniquenessChecker uniquenessChecker, IExerciseResourceLinkUniquenessChecker exerciseResourceLinkUniquenessChecker, IExerciseHierarchyChecker hierarchyChecker, IExerciseUsageChecker usageChecker)
    {
        _contracts = new() {
            uniquenessChecker = uniquenessChecker,
            exerciseResourceLinkUniquenessChecker = exerciseResourceLinkUniquenessChecker,
            hierarchyChecker = hierarchyChecker,
            usageChecker = usageChecker
        };
    }
}

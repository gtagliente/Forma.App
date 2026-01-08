using Forma.Domain.Builders.Contracts;
using Forma.Domain.Entities.ExerciseAggregate.Contracts;
using Shop.Domain.Entities.CustomerAggregate;

namespace Forma.Domain.Builders;
internal class ExerciseBuilder : IExerciseBuilder
{
    public IExerciseBuilder.Contracts _contracts {  get; init; }

    public ExerciseBuilder(IExerciseUniquenessChecker uniquenessChecker, IExerciseResourceLinkUniquenessChecker exerciseResourceLinkUniquenessChecker)
    {
        _contracts = new() {
            uniquenessChecker = uniquenessChecker,
            exerciseResourceLinkUniquenessChecker = exerciseResourceLinkUniquenessChecker
        };
    }
}

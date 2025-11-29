using Shop.Domain.Entities.CustomerAggregate;

namespace Forma.Domain.Builders.Contracts;

public interface IExerciseBuilder
{
    Contracts _contracts { internal get; init; }

    struct Contracts
    {
        public readonly IExerciseUniquenessChecker uniquenessChecker {  get; init; }
    }

}

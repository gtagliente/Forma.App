using System.Threading.Tasks;

namespace Forma.Domain.Entities.ExerciseAggregate.Contracts;

public interface IExerciseHierarchyChecker
{
    /// <summary>
    /// Checks whether an Exercise with the given Id exists.
    /// </summary>
    Task<bool> ExistsAsync(ExerciseId id);

    /// <summary>
    /// Checks whether making <paramref name="proposedParentId"/> the parent of <paramref name="childId"/>
    /// would create a cycle — i.e. whether <paramref name="childId"/> already appears in
    /// <paramref name="proposedParentId"/>'s ancestor chain.
    /// </summary>
    Task<bool> WouldCreateCycleAsync(ExerciseId childId, ExerciseId proposedParentId);

    /// <summary>
    /// Checks whether any Exercise declares <paramref name="id"/> as its parent.
    /// </summary>
    Task<bool> HasChildrenAsync(ExerciseId id);
}

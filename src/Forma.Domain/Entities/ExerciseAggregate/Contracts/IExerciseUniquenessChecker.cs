using System;
using System.Threading.Tasks;


namespace Forma.Domain.Entities.ExerciseAggregate.Contracts;

public interface IExerciseUniquenessChecker
{
    /// <summary>
    /// Checks if an exercise name is unique within the given ownership scope
    /// (null = shared library; non-null = that owner's private Exercises).
    /// </summary>
    /// <param name="name"></param>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    Task<bool> IsUniqueAsync(string name, Guid? ownerId);

}
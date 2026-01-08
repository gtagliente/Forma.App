using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forma.Domain.Entities.ExerciseAggregate.Contracts;

public interface IExerciseResourceLinkUniquenessChecker
{
    /// <summary>
    /// Checks if an exercise resource link is unique.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    Task<bool> IsUniqueExerciseResourceLinkAsync(string link);
}

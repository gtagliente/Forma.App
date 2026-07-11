using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Forma.Query.QueriesModel;
using Forma.Query.Abstractions;

namespace Forma.Query.Data.Repositories.Abstractions;

public interface IExerciseReadOnlyRepository : IReadOnlyRepository<ExerciseQueryModel, Guid>
{
    /// <summary>
    /// Returns every shared-library Exercise, plus the requesting user's own private Exercises.
    /// Pass null to see shared-library Exercises only.
    /// </summary>
    Task<IEnumerable<ExerciseQueryModel>> GetVisibleToAsync(Guid? requestingUserId);
}
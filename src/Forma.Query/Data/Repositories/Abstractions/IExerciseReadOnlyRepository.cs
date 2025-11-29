using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Forma.Query.QueriesModel;
using Forma.Query.Abstractions;

namespace Forma.Query.Data.Repositories.Abstractions;

public interface IExerciseReadOnlyRepository : IReadOnlyRepository<ExerciseQueryModel, Guid>
{
    Task<IEnumerable<ExerciseQueryModel>> GetAllAsync();
}
using Forma.CoreInfrastructure.Abstractions;
using Forma.Domain.Entities.ExerciseAggregate;
using Forma.Infrastructure.Data.Context;
using Forma.Infrastructure.Data.Repositories.Common;

namespace Forma.Infrastructure.Data.Repositories;

internal class ExerciseResourceWriteOnlyRepository(WriteDbContext dbContext)
    : BaseWriteOnlyRepository<ExerciseResource, ExerciseResourceId>(dbContext),
      IExerciseResourceWriteOnlyRepository<ExerciseResource, ExerciseResourceId>;

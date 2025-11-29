using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Forma.Infrastructure.Data.Context;
using Forma.Infrastructure.Data.Repositories.Common;
using Forma.Domain.Entities.ExerciseAggregate;
using Forma.CoreInfrastructure.Abstractions;
using Shop.Domain.Entities.CustomerAggregate;

namespace Forma.Infrastructure.Data.Repositories;

internal class ExerciseWriteOnlyRepository(WriteDbContext dbContext)
    : BaseWriteOnlyRepository<Exercise, Guid>(dbContext), IExerciseWriteOnlyRepository<Exercise, Guid>, IExerciseUniquenessChecker
{
    public async Task<bool> IsUniqueAsync(string name)
    {
        return await DbContext.Set<Exercise>()
            .AsNoTracking()
            .Where(e => e.Name == name)
            .CountAsync()>0;
    }
}
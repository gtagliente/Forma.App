using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Forma.CoreInfrastructure.Abstractions;
using Forma.Domain.Entities.ExerciseAggregate;
using Forma.Domain.Entities.ExerciseAggregate.Contracts;
using Forma.Infrastructure.Data.Context;
using Forma.Infrastructure.Data.Repositories.Common;
using Microsoft.EntityFrameworkCore;
using Shop.Domain.Entities.CustomerAggregate;

namespace Forma.Infrastructure.Data.Repositories;

internal class ExerciseWriteOnlyRepository(WriteDbContext dbContext)
    : BaseWriteOnlyRepository<Exercise, ExerciseId>(dbContext), IExerciseWriteOnlyRepository<Exercise, ExerciseId>, IExerciseUniquenessChecker, IExerciseResourceLinkUniquenessChecker
{
    public async Task<bool> IsUniqueAsync(string name)
    {
        return !await DbContext.Set<Exercise>()
            .AnyAsync(e => e.Name == name);
    }


    public async Task<bool> IsUniqueExerciseResourceLinkAsync(string link)
    {
        return !await DbContext.Set<ExerciseResource>()
            .AnyAsync(e => e.Link.Equals(link));
    }
}
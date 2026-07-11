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

namespace Forma.Infrastructure.Data.Repositories;

internal class ExerciseWriteOnlyRepository(WriteDbContext dbContext)
    : BaseWriteOnlyRepository<Exercise, ExerciseId>(dbContext), IExerciseWriteOnlyRepository<Exercise, ExerciseId>, IExerciseUniquenessChecker, IExerciseResourceLinkUniquenessChecker, IExerciseHierarchyChecker
{
    public async Task<bool> IsUniqueAsync(string name, Guid? ownerId)
    {
        return !await DbContext.Set<Exercise>()
            .AnyAsync(e => e.Name == name && e.OwnerId == ownerId);
    }


    public async Task<bool> IsUniqueExerciseResourceLinkAsync(string link)
    {
        return !await DbContext.Set<ExerciseResource>()
            .AnyAsync(e => e.Link.Equals(link));
    }

    public void AddResource(ExerciseResource resource) =>
        DbContext.Set<ExerciseResource>().Add(resource);

    public async Task<bool> ExistsAsync(ExerciseId id)
    {
        return await DbContext.Set<Exercise>()
            .AnyAsync(e => e.Id == id);
    }

    public async Task<bool> WouldCreateCycleAsync(ExerciseId childId, ExerciseId proposedParentId)
    {
        // Walk proposedParentId's ancestor chain one row at a time looking for childId.
        // Exercise libraries are curated, shallow trees — a straightforward loop is adequate;
        // no recursive CTE needed. Depth cap guards against a pre-existing corrupt cycle in the
        // data turning this into an infinite loop.
        const int maxDepth = 1000;

        ExerciseId? current = proposedParentId;
        for (var depth = 0; depth < maxDepth && current is not null; depth++)
        {
            if (current.Value == childId)
                return true;

            current = await DbContext.Set<Exercise>()
                .Where(e => e.Id == current.Value)
                .Select(e => e.ParentId)
                .FirstOrDefaultAsync();
        }
        return false;
    }

    public async Task<bool> HasChildrenAsync(ExerciseId id)
    {
        return await DbContext.Set<Exercise>()
            .AnyAsync(e => e.ParentId == id);
    }
}
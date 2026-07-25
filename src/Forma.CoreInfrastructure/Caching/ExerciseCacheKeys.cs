using System;

namespace Forma.CoreInfrastructure.Caching;

/// <summary>
/// Single shared builder for the GetAllExerciseQuery cache key, so the read-side handler that
/// writes the cache entry and every write-side path that must invalidate it agree on the exact
/// same key shape. Lives here (rather than in Forma.Query, where GetAllExerciseQuery itself is
/// declared) because both Forma.Application and Forma.Query already reference
/// Forma.CoreInfrastructure — putting it in Forma.Query would force Forma.Application to take a
/// new project reference it doesn't otherwise need.
/// </summary>
public static class ExerciseCacheKeys
{
    // Intentionally a literal, not nameof(GetAllExerciseQuery) — that type lives in Forma.Query,
    // which this project must not reference. Kept identical to the string nameof used to
    // produce, so no existing cache entries are orphaned by this refactor.
    private const string GetAllExerciseQueryName = "GetAllExerciseQuery";

    public static string ForUser(Guid? userId) => $"{GetAllExerciseQueryName}:{userId}";
}

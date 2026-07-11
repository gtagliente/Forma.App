using System;

namespace Forma.CoreInfrastructure.Abstractions;

public interface IExerciseResourceWriteOnlyRepository<TEntity, TKey> : IWriteOnlyRepository<TEntity, TKey>
    where TKey : IEquatable<TKey>;

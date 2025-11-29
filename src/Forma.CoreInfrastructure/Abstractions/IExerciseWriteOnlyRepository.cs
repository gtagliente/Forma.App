using System;


namespace Forma.CoreInfrastructure.Abstractions;

public interface IExerciseWriteOnlyRepository<TEntity, TKey> : IWriteOnlyRepository<TEntity, TKey>
    where TKey : IEquatable<TKey>
{

}

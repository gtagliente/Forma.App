using System.Threading.Tasks;


namespace Shop.Domain.Entities.CustomerAggregate;

public interface IExerciseUniquenessChecker
{
    /// <summary>
    /// Checks if an exercise name is unique.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    Task<bool> IsUniqueAsync(string name);

}
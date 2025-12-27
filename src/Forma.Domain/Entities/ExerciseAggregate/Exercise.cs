using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Forma.CoreContext.SharedKernel;
using Forma.Domain.Builders.Contracts;
using Forma.Domain.Entities.ExerciseAggregate.Events;
using Forma.Domain.Entities.ExerciseAggregate.ValueObjects;

namespace Forma.Domain.Entities.ExerciseAggregate;

public class Exercise : BaseEntity, IAggregateRoot
{
    public string Name { get; private set; } = default!;
    public string Description { get; private set; }

    // internal  collection (assign only inside the aggregate)
    private List<MuscleGroup> _muscleGroups = [];
    public IReadOnlyCollection<MuscleGroup> MuscleGroups => _muscleGroups;

    // Convenience property for a strongly typed ID
    public ExerciseId ExerciseId => new(Id);

    private IExerciseBuilder _builder;
    private Exercise(Guid id, string name, IEnumerable<MuscleGroup> muscleGroups, string description)
        : base(id)
    {
        Name = name;
        _muscleGroups = muscleGroups.ToList(); 
        Description = description;
    }


    private Exercise() { } // For EF or serialization

    public static async Task<Exercise> Create(IExerciseBuilder builder, string name, IEnumerable<MuscleGroup> muscleGroups, string description)
    {
        if (builder == null)
            throw new ArgumentNullException($"Required builder {nameof(builder)}");
        var contracts = builder._contracts;

        if (contracts.uniquenessChecker == null)
             throw new ArgumentException($"Required contract {nameof(contracts.uniquenessChecker)}");
        if(!await contracts.uniquenessChecker.IsUniqueAsync(name))
            throw new ArgumentException("An exercise with the same name already exists.");

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Exercise name is required.");
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Exercise description is required.");
        if (muscleGroups is null || muscleGroups.Count() == 0)
            throw new ArgumentException("At least one muscle group must be specified.");

        var exercise = new Exercise(Guid.NewGuid(), name, muscleGroups, description)
        {
            _builder = builder
        };
        exercise.AddDomainEvent(new ExerciseCreatedEvent(exercise.ExerciseId.Value, exercise.MuscleGroups, exercise.Name, exercise.Description));
        return exercise;
    }

    public async Task<bool> Update(string name = null, string description = null, IEnumerable<MuscleGroup> muscleGroups = null)
    {
        if (name is not null && string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.");

        var hasChanged = false;

        if (name is not null && name != Name)
        {
            if (!await _builder._contracts.uniquenessChecker.IsUniqueAsync(name))
                throw new ArgumentException("An exercise with the same name already exists.");
            Name = name.Trim();
            hasChanged = true;
        }

        if (description is not null && description != Description)
        {
            Description = description.Trim();
            hasChanged = true;
        }

        if (muscleGroups is not null)
        {
            if(muscleGroups.Count()==0)
                throw new ArgumentException("At least one muscle group must be specified.");
            if (!muscleGroups.SequenceEqual(_muscleGroups))
            {
                _muscleGroups = muscleGroups.ToList();
                hasChanged = true;
            }
        }

        if (hasChanged)
            AddDomainEvent(new ExerciseUpdatedEvent(ExerciseId.Value, MuscleGroups, Name, Description));
        return hasChanged;
    }
}

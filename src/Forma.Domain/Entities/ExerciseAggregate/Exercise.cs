using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Forma.CoreContext.SharedKernel;
using Forma.CoreContext.SharedKernel.Exceptions.DomainExceptions;
using Forma.Domain.Builders.Contracts;
using Forma.Domain.Entities.ExerciseAggregate.Events;
using Forma.Domain.Entities.ExerciseAggregate.ValueObjects;

namespace Forma.Domain.Entities.ExerciseAggregate;

public class Exercise : BaseEntity<ExerciseId>, IAggregateRoot
{
    public string Name { get; private set; } = default!;
    public string Description { get; private set; }

    // Null => shared-library Exercise, visible to everyone. Non-null => private, visible only to that user.
    public Guid? OwnerId { get; private set; }

    // References another Exercise aggregate by ID only (never a loaded parent object graph).
    public ExerciseId? ParentId { get; private set; }

    // internal  collection (assign only inside the aggregate)
    private List<MuscleGroup> _muscleGroups = [];
    public IReadOnlyCollection<MuscleGroup> MuscleGroups => _muscleGroups;

    private List<ExerciseResource> _resources = [];

    public IReadOnlyCollection<ExerciseResource> Resources => _resources;

    private Exercise(ExerciseId id, string name, IEnumerable<MuscleGroup> muscleGroups, string description, Guid? ownerId, ExerciseId? parentId)
        : base(id)
    {
        Name = name;
        _muscleGroups = muscleGroups.ToList();
        Description = description;
        OwnerId = ownerId;
        ParentId = parentId;
    }


    private Exercise() {
    } // For EF or serialization

    public static async Task<Exercise> Create(IExerciseBuilder builder, string name, IEnumerable<MuscleGroup> muscleGroups, string description, Guid? ownerId = null, ExerciseId? parentId = null)
    {
        if (builder == null)
            throw new DomainBadCodeException($"Required builder {nameof(builder)}");
        var contracts = builder._contracts;

        if (contracts.uniquenessChecker == null)
             throw new DomainBadCodeException($"Required contract {nameof(contracts.uniquenessChecker)}");
        if(!await contracts.uniquenessChecker.IsUniqueAsync(name, ownerId))
            throw new DomainArgumentException("An exercise with the same name already exists.");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainArgumentException("Exercise name is required.");
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainArgumentException("Exercise description is required.");
        if (muscleGroups is null || muscleGroups.Count() == 0)
            throw new DomainArgumentException("At least one muscle group must be specified.");

        if (parentId is not null)
        {
            if (contracts.hierarchyChecker == null)
                throw new DomainBadCodeException($"Required contract {nameof(contracts.hierarchyChecker)}");
            if (!await contracts.hierarchyChecker.ExistsAsync(parentId.Value))
                throw new DomainArgumentException($"No exercise found by Id: {parentId}");
        }

        var exercise = new Exercise(ExerciseId.New(), name, muscleGroups, description, ownerId, parentId);

        exercise.AddDomainEvent(new ExerciseCreatedEvent(exercise.Id, exercise.MuscleGroups, exercise.Name, exercise.Description, exercise.OwnerId));
        return exercise;
    }

    public async Task<bool> Update(IExerciseBuilder builder, string name = null, string description = null, IEnumerable<MuscleGroup> muscleGroups = null)
    {
        if (name is not null && string.IsNullOrWhiteSpace(name))
            throw new DomainArgumentException("Name cannot be empty.");

        var hasChanged = false;

        if (name is not null && name != Name)
        {
            if (!await builder._contracts.uniquenessChecker.IsUniqueAsync(name, OwnerId))
                throw new DomainArgumentException("An exercise with the same name already exists.");
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
                throw new DomainArgumentException("At least one muscle group must be specified.");
            if (!muscleGroups.SequenceEqual(_muscleGroups))
            {
                _muscleGroups = muscleGroups.ToList();
                hasChanged = true;
            }
        }

        if (hasChanged)
            AddDomainEvent(new ExerciseUpdatedEvent(Id, MuscleGroups, Name, Description, OwnerId));
        return hasChanged;
    }

    public async Task<ExerciseResource> AddResource(IExerciseBuilder builder, string title, string content, ResourceType type, string link)
    {
        if(builder == null)
            throw new DomainBadCodeException($"Required builder {nameof(builder)}");
        if  (
            //Cerco prima nelle entità già presenti per evitare chiamate inutili al db
            Resources.Any( r => r.Link.Equals(link, StringComparison.CurrentCultureIgnoreCase))
                    ||
            !await builder._contracts.exerciseResourceLinkUniquenessChecker.IsUniqueExerciseResourceLinkAsync(link)
            )
            throw new DomainArgumentException("A resource with the same link already exists for this exercise.");

        var resource = ExerciseResource.Create(builder, Id, title, content, type, link);
        _resources.Add(resource);
        //AddDomainEvent(new ExerciseDetailAddedEvent(ExerciseId.Value, detail.ExerciseDetailId.Value, detail.Title, detail.Type, detail.Link));
        return resource;
    }

    public async Task<bool> RemoveResource(ExerciseResourceId detailId)
    {
        var resource = _resources.FirstOrDefault(d => d.Id == detailId);
        if (resource == null)
            return false;
        _resources.Remove(resource);
        //AddDomainEvent(new ExerciseDetailRemovedEvent(ExerciseId.Value, detail.ExerciseDetailId.Value));
        return true;
    }

    public async Task SetParent(IExerciseBuilder builder, ExerciseId parentId)
    {
        if (builder == null)
            throw new DomainBadCodeException($"Required builder {nameof(builder)}");
        var checker = builder._contracts.hierarchyChecker;
        if (checker == null)
            throw new DomainBadCodeException($"Required contract {nameof(builder._contracts.hierarchyChecker)}");

        if (parentId == Id)
            throw new DomainArgumentException("An exercise cannot be its own parent.");

        if (!await checker.ExistsAsync(parentId))
            throw new DomainArgumentException($"No exercise found by Id: {parentId}");

        if (await checker.WouldCreateCycleAsync(Id, parentId))
            throw new DomainArgumentException("Assigning this parent would create a cycle in the exercise hierarchy.");

        ParentId = parentId;
    }

    public void ClearParent() => ParentId = null;

    public async Task Delete(IExerciseBuilder builder)
    {
        if (builder == null)
            throw new DomainBadCodeException($"Required builder {nameof(builder)}");
        var checker = builder._contracts.hierarchyChecker;
        if (checker == null)
            throw new DomainBadCodeException($"Required contract {nameof(builder._contracts.hierarchyChecker)}");

        if (await checker.HasChildrenAsync(Id))
            throw new DomainArgumentException("Cannot delete an exercise that has children in the hierarchy.");

        AddDomainEvent(new ExerciseDeletedEvent(Id, MuscleGroups, Name, Description, OwnerId));
    }
}

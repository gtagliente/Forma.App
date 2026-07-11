using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Forma.Domain.Entities.ExerciseAggregate;
using Forma.Domain.Entities.ExerciseAggregate.ValueObjects;
using Forma.Infrastructure.Data.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Forma.Infrastructure.Data.Mappings;

internal class ExerciseConfiguration : IEntityTypeConfiguration<Exercise>
{
    public void Configure(EntityTypeBuilder<Exercise> builder)
    {
        //TODO: rifattorizzare con baseentity e typed id passato tramite Generics
        builder
            .ConfigureBaseEntity<Exercise,ExerciseId>();

      
        builder
            .Property(entity => entity.Id)
            .HasConversion(e => e.Value, value => new(value))
            .IsRequired()
            .ValueGeneratedNever();

        builder
            .Property(exercise => exercise.Name)
            .IsRequired() // NOT NULL
            .HasMaxLength(100);

        builder
            .Property(exercise => exercise.OwnerId);

        builder
            .Property(exercise => exercise.ParentId)
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? new ExerciseId(value.Value) : (ExerciseId?)null);

        // Self-referencing: child holds a reference to the parent's ID only (see Exercise.ParentId).
        // Restrict, not Cascade/SetNull: FT-003 (Delete) must explicitly decide what happens when
        // deleting an Exercise that still has children, not have it happen silently via FK behavior.
        builder
            .HasOne<Exercise>()
            .WithMany()
            .HasForeignKey(e => e.ParentId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Exercise_Parent");


        // Converter: IReadOnlyCollection<MuscleGroup> <-> "1|2|5"
        var muscleGroupConverter = new ValueConverter<IReadOnlyCollection<MuscleGroup>, string>(
             // to database
             v => ConvertMuscleGroupsToString(v),
            v => ConvertStringToMuscleGroups(v));



        // Comparer for change-tracking (sequence equality + stable hash)
        var muscleGroupComparer = new ValueComparer<IReadOnlyCollection<MuscleGroup>>(
            (a, b) => EqualsExpression(a, b),
            v => HashCodeExpression(v),
            v => SnapshotExpression(v)
        );

        builder
            .Property(exercise => exercise.MuscleGroups)
            .HasConversion(muscleGroupConverter)
            .HasMaxLength(200)
            .IsRequired()
            .HasField(typeof(Exercise).GetField("_muscleGroups", BindingFlags.NonPublic | BindingFlags.Instance)?.Name ?? throw new Exception("Campo non trovato"))
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .Metadata.SetValueComparer(muscleGroupComparer);


        builder
            .Property(exercise => exercise.Description)
            .HasMaxLength(100);

        builder
             .Property(exercise => exercise.RowVersion)
             .IsRequired()
             .IsRowVersion();

        builder.HasMany(e => e.Resources)
                .WithOne()
                .HasForeignKey(r => r.ExerciseId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Exercise_ExerciseResources");

        // Name uniqueness is scoped by ownership, not global: shared-library names must be
        // unique among themselves, and each owner's private names must be unique among that
        // owner's own Exercises — a private name may coincide with a shared or another owner's name.
        builder
            .HasIndex(e => e.Name)
            .IsUnique()
            .HasFilter("[OwnerId] IS NULL")
            .HasDatabaseName("UQ_Exercise_Name_Shared");

        builder
            .HasIndex(e => new { e.Name, e.OwnerId })
            .IsUnique()
            .HasFilter("[OwnerId] IS NOT NULL")
            .HasDatabaseName("UQ_Exercise_Name_PerOwner");
    }

    private static string ConvertMuscleGroupsToString(IReadOnlyCollection<MuscleGroup> mem)
    {
        if (!mem.Any()) return string.Empty;
        return string.Join('|', mem.Select(m => ((int)m).ToString()));
    }

    private static List<MuscleGroup> ConvertStringToMuscleGroups(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return Array.Empty<MuscleGroup>().ToList();
        var arr = s.Split('|', StringSplitOptions.RemoveEmptyEntries)
                   .Select(token => (MuscleGroup)int.Parse(token))
                   .ToList();
        return arr;
    }

    private static bool EqualsExpression(IReadOnlyCollection<MuscleGroup> a, IReadOnlyCollection<MuscleGroup> b)
    {
        if (!a.Any() && !b.Any()) return true;
        if (a.Count != b.Count) return false;
        return a.SequenceEqual(b);
    }

    private static int HashCodeExpression(IReadOnlyCollection<MuscleGroup> v)
    {
        if (!v.Any()) return 0;
        var h = 17;
        foreach (var mg in v)
            h = h * 23 + ((int)mg).GetHashCode();
        return h;
    }

    private static IReadOnlyCollection<MuscleGroup> SnapshotExpression(IReadOnlyCollection<MuscleGroup> v)
    {
        // snapshot as a Memory backed by a new array
        if (!v.Any()) return Array.Empty<MuscleGroup>().AsReadOnly();
        return v.ToArray().AsReadOnly();
    }
}
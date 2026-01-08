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
            .ConfigureBaseEntity();

        builder
            .HasKey(e => e.ExerciseId);

        builder
            .Property(entity => entity.ExerciseId)
            .HasConversion(e => e.Value, value => new(value))
            .IsRequired()
            .ValueGeneratedNever();

        builder
            .Property(exercise => exercise.Name)
            .IsRequired() // NOT NULL
            .HasMaxLength(100);


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

        builder
            .HasIndex(e => e.Name)
            .IsUnique()
            .HasDatabaseName("UQ_Exercise_Name");
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
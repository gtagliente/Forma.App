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
        builder
            .ConfigureBaseEntity();

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
        //// Value Object Mapping (ValueObject)
        //builder.OwnsOne(customer => customer.Email, ownedNav =>
        //{
        //    ownedNav
        //        .Property(email => email.Address)
        //        .IsRequired() // NOT NULL
        //        .HasMaxLength(254)
        //        .HasColumnName(nameof(Customer.Email));

        //    // Unique Index
        //    ownedNav
        //        .HasIndex(email => email.Address)
        //        .IsUnique();
        //});

        //builder
        //    .Property(customer => customer.DateOfBirth)
        //    .IsRequired() // NOT NULL
        //    .HasColumnType("DATE");
    }

    private static string ConvertMuscleGroupsToString(IReadOnlyCollection<MuscleGroup> mem)
    {
        if (!mem.Any()) return string.Empty;
        return string.Join('|', mem.Select(m => ((int)m).ToString()));
    }

    private static IReadOnlyCollection<MuscleGroup> ConvertStringToMuscleGroups(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return Array.Empty<MuscleGroup>().AsReadOnly();
        var arr = s.Split('|', StringSplitOptions.RemoveEmptyEntries)
                   .Select(token => (MuscleGroup)int.Parse(token))
                   .ToArray();
        return arr.AsReadOnly();
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
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Forma.Domain.Entities.ExerciseAggregate;
using Forma.Infrastructure.Data.Extensions;

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

        builder
            .Property(exersice => exersice.MuscleGroup)
            .IsRequired() // NOT NULL
            .HasMaxLength(10)
            .HasConversion<string>();

        builder
            .Property(exercise => exercise.Description)
            .HasMaxLength(100);

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
}
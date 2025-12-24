using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Forma.Domain.Entities.ExerciseAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Forma.Infrastructure.Data.Mappings;

public class StaticValueObjects
{
    public int Id { get; set; }
    public string Key { get; set; }
    public string Value { get; set; }
}
internal class StaticValueObjectsConfiguration : IEntityTypeConfiguration<StaticValueObjects>
{
    public void Configure(EntityTypeBuilder<StaticValueObjects> builder)
    {
        builder
            .HasKey(entity => entity.Id);

        builder
            .Property(sv => sv.Id)
            .IsRequired()
            .ValueGeneratedOnAdd();

        builder
            .Property(sv => sv.Key)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(sv => sv.Value)
            .IsRequired()
            .HasColumnType("nvarchar(max)");
    }
}

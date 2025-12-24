using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Forma.Domain.Entities.ExerciseAggregate.ValueObjects;
using Forma.Infrastructure.Data.Context;
using Forma.Infrastructure.Data.Mappings;
using Microsoft.EntityFrameworkCore;

namespace Forma.Infrastructure.Data.Services.Seeders;

public static class StaticValueObjectsSeeder
{
    public static async Task SeedAsync(WriteDbContext db)
    {
        var strategy = db.Database.CreateExecutionStrategy();

        // Executing the strategy.
        await strategy.ExecuteAsync(async () =>
        {
            var actualEntites = await db.StaticValueObjects
                        .ToListAsync();

            var values = Enum.GetValues(typeof(MuscleGroup)).Cast<MuscleGroup>()
                            .Select(v => new
                            {
                                Id = (int)v,
                                Value = v.ToString(),
                            });
            var jsonValue = JsonSerializer.Serialize(values);

            var muscleGroupsEntity = actualEntites.Where(e => e.Key.Equals("MuscleGroups")).FirstOrDefault();
            if (muscleGroupsEntity != null)
                muscleGroupsEntity.Value = jsonValue;
            else
            {
                muscleGroupsEntity = new StaticValueObjects
                {
                    Key = "MuscleGroups",
                    Value = jsonValue
                };
                db.StaticValueObjects.Add(muscleGroupsEntity);
            }
            await db.SaveChangesAsync();
        });
    }




    //public static void Main()
    //{
    //    var values = (Enum.GetValues(typeof(MuscleGroup)).Cast<MuscleGroup>())
    //                    .Select(v => new {
    //                        Id = (int)v,
    //                        Value = v.ToString(),
    //                        Descr = Program.GetEnumDescription((MuscleGroup)v)
    //                    });
    //    Console.WriteLine("Hello World " + string.Join(",", values));
    //}


    //public static string GetEnumDescription(MuscleGroup value)
    //{
    //    FieldInfo fi = value.GetType().GetField(value.ToString());

    //    DescriptionAttribute[] attributes = fi.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];

    //    if (attributes != null && attributes.Any())
    //    {
    //        return attributes.First().Description;
    //    }

    //    return value.ToString();
    //}
}

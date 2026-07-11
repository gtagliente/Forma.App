using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Ardalis.Result;
using Forma.Domain.Entities.ExerciseAggregate;
using Forma.Domain.Entities.ExerciseAggregate.ValueObjects;
using MediatR;

namespace Forma.Application.Exercise.Commands;

public class UpdateExerciseCommand : IRequest<Result>
{
    [Required]
    public ExerciseId ExerciseId { get; set; }

    [MaxLength(100)]
    [DataType(DataType.Text)]
    public string Name { get; set; }

    [MaxLength(100)]
    [DataType(DataType.Text)]
    public string Description { get; set; }

    public IEnumerable<MuscleGroup> MuscleGroups { get; set; }
}

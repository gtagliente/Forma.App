using System.ComponentModel.DataAnnotations;
using Ardalis.Result;
using Forma.Domain.Entities.ExerciseAggregate.ValueObjects;
using MediatR;
using Forma.Application.Exercise.Responses;
using System.Collections.Generic;

namespace Forma.Application.Exercise.Commands;

public class CreateExerciseCommand : IRequest<Result<CreatedExerciseResponse>>
{
    [Required]
    [MaxLength(100)]
    [DataType(DataType.Text)]
    public string Name { get; set; }

    [Required]
    [MaxLength(100)]
    [DataType(DataType.Text)]
    public string Description { get; set; }

    [Required]
    public IEnumerable<MuscleGroup> MuscleGroups { get; set; }

}
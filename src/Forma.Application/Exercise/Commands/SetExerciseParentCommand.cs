using System.ComponentModel.DataAnnotations;
using Ardalis.Result;
using Forma.Domain.Entities.ExerciseAggregate;
using MediatR;

namespace Forma.Application.Exercise.Commands;

public class SetExerciseParentCommand : IRequest<Result>
{
    [Required]
    public ExerciseId ExerciseId { get; set; }

    [Required]
    public ExerciseId ParentId { get; set; }
}

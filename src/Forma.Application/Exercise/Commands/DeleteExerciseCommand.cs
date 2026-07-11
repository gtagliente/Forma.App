using System.ComponentModel.DataAnnotations;
using Ardalis.Result;
using Forma.Domain.Entities.ExerciseAggregate;
using MediatR;

namespace Forma.Application.Exercise.Commands;

public class DeleteExerciseCommand(ExerciseId exerciseId) : IRequest<Result>
{
    [Required]
    public ExerciseId ExerciseId { get; } = exerciseId;
}

using FluentValidation;

namespace Forma.Application.Exercise.Commands;

public class DeleteExerciseCommandValidator : AbstractValidator<DeleteExerciseCommand>
{
    public DeleteExerciseCommandValidator()
    {
        RuleFor(command => command.ExerciseId)
            .NotEmpty();
    }
}

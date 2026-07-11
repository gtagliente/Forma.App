using FluentValidation;

namespace Forma.Application.Exercise.Commands;

public class ClearExerciseParentCommandValidator : AbstractValidator<ClearExerciseParentCommand>
{
    public ClearExerciseParentCommandValidator()
    {
        RuleFor(command => command.ExerciseId)
            .NotEmpty();
    }
}

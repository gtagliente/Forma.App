using FluentValidation;

namespace Forma.Application.Exercise.Commands;

public class SetExerciseParentCommandValidator : AbstractValidator<SetExerciseParentCommand>
{
    public SetExerciseParentCommandValidator()
    {
        RuleFor(command => command.ExerciseId)
            .NotEmpty();

        RuleFor(command => command.ParentId)
            .NotEmpty();
    }
}

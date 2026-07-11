using FluentValidation;

namespace Forma.Application.Exercise.Commands;

public class UpdateExerciseCommandValidator : AbstractValidator<UpdateExerciseCommand>
{
    public UpdateExerciseCommandValidator()
    {
        RuleFor(command => command.ExerciseId)
            .NotEmpty();

        RuleFor(command => command.Name)
            .MaximumLength(100)
            .When(command => command.Name is not null);

        RuleFor(command => command.Description)
            .MaximumLength(100)
            .When(command => command.Description is not null);
    }
}

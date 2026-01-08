using FluentValidation;

namespace Forma.Application.Exercise.Commands;

public class CreateExerciseCommandValidator : AbstractValidator<CreateExerciseCommand>
{
    public CreateExerciseCommandValidator() 
    {
        RuleFor(command => command.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(command => command.Description)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(command => command.MuscleGroups)
            .NotEmpty();
    }
}
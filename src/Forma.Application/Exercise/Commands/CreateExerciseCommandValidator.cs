using FluentValidation;

namespace Forma.Application.Exercise.Commands;

public class CreateCustomerCommandValidator : AbstractValidator<CreateExerciseCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(command => command.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(command => command.Description)
            .NotEmpty()
            .MaximumLength(100);

        //RuleFor(command => command.MuscleGroup)
        //    .NotEmpty();
    }
}
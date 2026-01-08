using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;

namespace Forma.Application.Exercise.Commands;

public class CreateExerciseResourceCommandValidator : AbstractValidator<CreateExerciseResourceCommand>
{
    public CreateExerciseResourceCommandValidator()
    {
        RuleFor(command  => command.ExerciseId)
            //.Custom((e,a) => e.Value != Guid.Empty)
            .NotEmpty();

        RuleFor(command => command.Title)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(command => command.Type)
             .IsInEnum();

        RuleFor(command => command.Link)
                .NotEmpty();
    }

}


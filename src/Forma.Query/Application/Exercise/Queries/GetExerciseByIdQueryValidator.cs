using FluentValidation;

namespace Forma.Query.Application.Exercise.Queries;

public class GetExerciseByIdQueryValidator : AbstractValidator<GetExerciseByIdQuery>
{
    public GetExerciseByIdQueryValidator()
    {
        RuleFor(query => query.Id)
            .NotEmpty();
    }
}

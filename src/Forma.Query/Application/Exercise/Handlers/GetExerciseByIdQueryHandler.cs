using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Ardalis.Result.FluentValidation;
using FluentValidation;
using Forma.Query.Application.Exercise.Queries;
using Forma.Query.Data.Repositories.Abstractions;
using Forma.Query.QueriesModel;
using MediatR;

namespace Forma.Query.Application.Exercise.Handlers;

public class GetExerciseByIdQueryHandler(
    IValidator<GetExerciseByIdQuery> validator,
    IExerciseReadOnlyRepository repository) : IRequestHandler<GetExerciseByIdQuery, Result<ExerciseQueryModel>>
{
    public async Task<Result<ExerciseQueryModel>> Handle(GetExerciseByIdQuery request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return Result<ExerciseQueryModel>.Invalid(validationResult.AsErrors());

        var exercise = await repository.GetByIdAsync(request.Id);
        if (exercise == null)
            return Result<ExerciseQueryModel>.NotFound($"Exercise with Id {request.Id} not found");

        return Result<ExerciseQueryModel>.Success(exercise);
    }
}

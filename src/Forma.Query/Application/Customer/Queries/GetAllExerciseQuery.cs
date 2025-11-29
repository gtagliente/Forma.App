using System.Collections.Generic;
using Ardalis.Result;
using MediatR;
using Forma.Query.QueriesModel;

namespace Forma.Query.Application.Exercise.Queries;

public class GetAllExerciseQuery : IRequest<Result<IEnumerable<ExerciseQueryModel>>>;
using System;
using Forma.CoreInfrastructure.Abstractions;

namespace Forma.Application.Exercise.Responses;

public class CreatedExerciseResourceResponse(Guid id) : IResponse
{
    public Guid Id { get; } = id;
}
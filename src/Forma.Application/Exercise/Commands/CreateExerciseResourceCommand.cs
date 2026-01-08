using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ardalis.Result;
using Forma.Application.Exercise.Responses;
using Forma.Domain.Entities.ExerciseAggregate;
using Forma.Domain.Entities.ExerciseAggregate.ValueObjects;
using MediatR;

namespace Forma.Application.Exercise.Commands;

public class CreateExerciseResourceCommand : IRequest<Result<CreatedExerciseResourceResponse>>
{
    [Required]
    public ExerciseId ExerciseId { get;  set; }

    [Required]
    [MaxLength(100)]
    [DataType(DataType.Text)]
    public string Title { get;  set; }

    [MaxLength(1000)]
    [DataType(DataType.Text)]
    public string Content { get;  set; }

    [Required]
    public ResourceType Type { get;  set; }

    [Required]
    [MaxLength(400)]
    [DataType(DataType.Url)]
    public string Link { get;  set; }
}

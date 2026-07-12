using System;
using System.ComponentModel.DataAnnotations;
using Ardalis.Result;
using Forma.Domain.Entities.ExerciseAggregate;
using Forma.Domain.Entities.ExerciseAggregate.ValueObjects;
using MediatR;
using Forma.Application.Exercise.Responses;
using System.Collections.Generic;

namespace Forma.Application.Exercise.Commands;

public class CreateExerciseCommand : IRequest<Result<CreatedExerciseResponse>>
{
    [Required]
    [MaxLength(100)]
    [DataType(DataType.Text)]
    public string Name { get; set; }

    [Required]
    [MaxLength(100)]
    [DataType(DataType.Text)]
    public string Description { get; set; }

    [Required]
    public IEnumerable<MuscleGroup> MuscleGroups { get; set; }

    /// <summary>
    /// Null creates a shared-library Exercise (visible to everyone).
    /// Non-null creates a private Exercise owned by that user (visible only to them).
    /// No auth exists yet, so this is caller-supplied — see docs/features/FT-001-ownership-visibility.md (Design section).
    /// </summary>
    public Guid? OwnerId { get; set; }

    /// <summary>
    /// Optional parent Exercise, forming a generalization/specialization relationship
    /// (e.g. "Barbell Bench Press" specializing "Bench Press"). Must reference an existing Exercise.
    /// </summary>
    public ExerciseId? ParentId { get; set; }

}
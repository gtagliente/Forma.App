using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
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
    /// True creates a shared-library Exercise (visible to everyone).
    /// False creates a private Exercise owned by the authenticated caller (visible only to them).
    /// The only client-supplied signal for shared-vs-mine — never an identity-bearing value.
    /// See ADR-007-jwt-bearer-authentication.md.
    /// </summary>
    public bool Shared { get; set; }

    /// <summary>
    /// Null creates a shared-library Exercise. Non-null creates a private Exercise owned by that
    /// user. Never bindable from the request body — the controller sets this after model binding,
    /// from the authenticated caller's id (ICurrentUserAccessor) or null (per Shared), never a
    /// caller-asserted value. See ADR-007-jwt-bearer-authentication.md.
    /// </summary>
    [JsonIgnore]
    public Guid? OwnerId { get; set; }

    /// <summary>
    /// Optional parent Exercise, forming a generalization/specialization relationship
    /// (e.g. "Barbell Bench Press" specializing "Bench Press"). Must reference an existing Exercise.
    /// </summary>
    public ExerciseId? ParentId { get; set; }

}
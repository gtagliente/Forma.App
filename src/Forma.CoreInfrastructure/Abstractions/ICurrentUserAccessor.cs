using System;

namespace Forma.CoreInfrastructure.Abstractions;

/// <summary>
/// Exposes the identity of the currently authenticated caller, derived from the validated
/// JWT bearer token (see ADR-007-jwt-bearer-authentication.md). Controllers and MediatR
/// command/query handlers use this instead of reading ClaimsPrincipal directly.
/// </summary>
public interface ICurrentUserAccessor
{
    /// <summary>
    /// The authenticated user's id, parsed from the token's raw "sub" claim.
    /// Null when there is no authenticated caller — never coalesced to Guid.Empty or any
    /// other default Guid, since that would silently collide with the shared-library
    /// "OwnerId == null" visibility rule.
    /// </summary>
    Guid? UserId { get; }

    /// <summary>
    /// True when the current request carries a validated identity.
    /// </summary>
    bool IsAuthenticated { get; }
}

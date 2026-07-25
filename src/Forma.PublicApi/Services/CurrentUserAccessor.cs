using System;
using Microsoft.AspNetCore.Http;
using Forma.CoreInfrastructure.Abstractions;

namespace Forma.PublicApi.Services;

/// <summary>
/// Wraps IHttpContextAccessor to expose the current request's authenticated user, per
/// ADR-007-jwt-bearer-authentication.md. Reads the raw "sub" claim (MapInboundClaims = false
/// on the JWT bearer options keeps it from being remapped to a ClaimTypes.NameIdentifier URI).
/// </summary>
internal sealed class CurrentUserAccessor(IHttpContextAccessor httpContextAccessor) : ICurrentUserAccessor
{
    public Guid? UserId
    {
        get
        {
            var subClaimValue = httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value;
            return Guid.TryParse(subClaimValue, out var userId) ? userId : null;
        }
    }

    public bool IsAuthenticated =>
        httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;
}

using System.ComponentModel.DataAnnotations;
using Forma.CoreInfrastructure.Abstractions;

namespace Forma.CoreInfrastructure.AppSettings;

/// <summary>
/// JWT bearer authentication settings — see
/// Forma.Claude/docs/architecture/adr/ADR-007-jwt-bearer-authentication.md.
/// The signing key must match identity-service's fastapi-users JWTStrategy secret.
/// </summary>
public sealed class JwtOptions : IAppOptions
{
    static string IAppOptions.ConfigSectionPath => "Auth";

    [Required]
    public string JwtSigningKey { get; private init; }
}

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Forma.IntegrationTests.Infrastructure;

/// <summary>
/// Mints tokens shaped like identity-service's real fastapi-users JWTStrategy output (HS256,
/// "sub"/"aud"/"exp" claims, no "iss") so integration tests can exercise [Authorize]-guarded
/// endpoints. Signing key must match appsettings.IntegrationTesting.json's Auth:JwtSigningKey.
/// See ADR-007-jwt-bearer-authentication.md.
/// </summary>
public static class TestJwtTokenFactory
{
    private const string SigningKey = "integration-testing-only-signing-key-not-a-real-secret";
    private const string Audience = "fastapi-users:auth";

    public static string CreateToken(Guid? userId = null)
    {
        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey)),
            SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim> { new("sub", (userId ?? Guid.NewGuid()).ToString()) };

        var token = new JwtSecurityToken(
            issuer: null,
            audience: Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

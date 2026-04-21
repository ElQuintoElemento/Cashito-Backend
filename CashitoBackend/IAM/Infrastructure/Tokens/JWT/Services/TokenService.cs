using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;
using System.Text;
using CashitoBackend.IAM.Application.Internal.OutboundServices;
using CashitoBackend.IAM.Domain.Model.Aggregates;
using CashitoBackend.IAM.Infrastructure.Tokens.JWT.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CashitoBackend.IAM.Infrastructure.Tokens.JWT.Services;

public class TokenService(IOptions<TokenSettings> tokenSettings) : ITokenService
{
    private readonly TokenSettings _tokenSettings = tokenSettings.Value;

    public string GenerateToken(User user)
    {
        var key = Encoding.ASCII.GetBytes(_tokenSettings.Secret);

        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Sid, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username)
        });

        // roles
        identity.AddClaims(
            user.Roles.Select(r =>
                new Claim(ClaimTypes.Role, r.Name.ToString())
            )
        );

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = identity,
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature
            )
        };

        return new JsonWebTokenHandler().CreateToken(tokenDescriptor);
    }

    public async Task<int?> ValidateToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        var key = Encoding.ASCII.GetBytes(_tokenSettings.Secret);

        try
        {
            var result = await new JsonWebTokenHandler().ValidateTokenAsync(
                token,
                new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }
            );

            var jwt = (JsonWebToken)result.SecurityToken;

            var userId = jwt.Claims
                .First(x => x.Type == ClaimTypes.Sid)
                .Value;

            return int.Parse(userId);
        }
        catch
        {
            return null;
        }
    }
}
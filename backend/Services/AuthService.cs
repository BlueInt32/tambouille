using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Tambouille.Services;

public class AuthService(IConfiguration configuration)
{
    private readonly string _password = configuration["Auth:Password"]
        ?? throw new InvalidOperationException("Auth:Password not configured");
    private readonly string _jwtSecret = configuration["Auth:JwtSecret"]
        ?? throw new InvalidOperationException("Auth:JwtSecret not configured");

    public bool CheckPassword(string password) =>
        password == _password;

    public string GenerateToken()
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: [new Claim(ClaimTypes.Name, "user")],
            expires: DateTime.UtcNow.AddDays(30),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

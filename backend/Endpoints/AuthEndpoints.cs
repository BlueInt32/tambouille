using Tambouille.Services;

namespace Tambouille.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        app.MapPost("/api/auth/login", (LoginRequest request, AuthService authService, ILogger<AuthService> logger) =>
        {
            if (!authService.CheckPassword(request.Password))
            {
                logger.LogWarning("Failed login attempt");
                return Results.Unauthorized();
            }

            var token = authService.GenerateToken();
            logger.LogInformation("Successful login");
            return Results.Ok(new { token });
        });
    }
}

public record LoginRequest(string Password);

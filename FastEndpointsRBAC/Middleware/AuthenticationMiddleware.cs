using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using FastEndpointsRBAC.Services;

namespace FastEndpointsRBAC.Middleware;

public class AuthenticationMiddleware : IMiddleware
{
    private readonly JwtService _jwtService;

    public AuthenticationMiddleware(JwtService jwtService)
    {
        _jwtService = jwtService;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
        if (token != null)
        {
            // Validate and set user claims
            // This is a simplified example. In a real-world scenario, you'd validate the token properly.
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

            if (jsonToken != null)
            {
                var claims = jsonToken.Claims;
                context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));
            }
        }

        await next(context);
    }
}
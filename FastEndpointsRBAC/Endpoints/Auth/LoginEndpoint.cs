using FastEndpoints;

using FastEndpointsRBAC.Data;
using FastEndpointsRBAC.Requests;
using FastEndpointsRBAC.Responses;
using FastEndpointsRBAC.Services;

using Microsoft.EntityFrameworkCore;

namespace FastEndpointsRBAC.Endpoints.Auth;

public class LoginEndpoint : Endpoint<LoginRequest, LoginResponse>
{
    private readonly AppDbContext _dbContext;
    private readonly JwtService _jwtService;

    public LoginEndpoint(AppDbContext dbContext, JwtService jwtService)
    {
        _dbContext = dbContext;
        _jwtService = jwtService;
    }

    public override void Configure()
    {
        Post("/auth/login");
        AllowAnonymous();
    }

    public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
    {
        var user = await _dbContext.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .ThenInclude(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Username == req.Username || u.Email == req.Username, ct);

        if (user == null)
        {
            await SendAsync(new LoginResponse
            {
                Success = false,
                Message = "User not found"
            }, 200, ct);
            return;
        }

        if (!BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
        {
            await SendAsync(new LoginResponse
            {
                Success = false,
                Message = "Invalid password"
            }, 200, ct);
            return;
        }

        if (!user.IsActive)
        {
            await SendAsync(new LoginResponse
            {
                Success = false,
                Message = "User account is not active"
            }, 200, ct);
            return;
        }

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var permissions = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Name)
            .Distinct()
            .ToList();

        var token = _jwtService.GenerateToken(user, roles, permissions);
        var refreshToken = _jwtService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _dbContext.SaveChangesAsync(ct);

        await SendAsync(new LoginResponse
        {
            Success = true,
            Token = token,
            RefreshToken = refreshToken
        }, 200, ct);
    }
}
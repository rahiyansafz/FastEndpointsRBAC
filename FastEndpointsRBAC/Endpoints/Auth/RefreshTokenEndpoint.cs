using FastEndpoints;

using FastEndpointsRBAC.Data;
using FastEndpointsRBAC.Requests;
using FastEndpointsRBAC.Responses;
using FastEndpointsRBAC.Services;

using Microsoft.EntityFrameworkCore;

namespace FastEndpointsRBAC.Endpoints.Auth;

public class RefreshTokenEndpoint : Endpoint<RefreshTokenRequest, LoginResponse>
{
    private readonly AppDbContext _dbContext;
    private readonly JwtService _jwtService;

    public RefreshTokenEndpoint(AppDbContext dbContext, JwtService jwtService)
    {
        _dbContext = dbContext;
        _jwtService = jwtService;
    }

    public override void Configure()
    {
        Post("/auth/refresh-token");
        AllowAnonymous();
    }

    public override async Task HandleAsync(RefreshTokenRequest req, CancellationToken ct)
    {
        var user = await _dbContext.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .ThenInclude(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.RefreshToken == req.RefreshToken && u.RefreshTokenExpiryTime > DateTime.UtcNow, ct);

        if (user == null)
        {
            await SendUnauthorizedAsync(ct);
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
            Token = token,
            RefreshToken = refreshToken
        }, cancellation: ct);
    }
}
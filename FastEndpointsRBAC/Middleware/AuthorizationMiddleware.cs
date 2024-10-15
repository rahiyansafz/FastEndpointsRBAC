using System.Security.Claims;

using FastEndpointsRBAC.Data;
using FastEndpointsRBAC.MetaData;

using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace FastEndpointsRBAC.Middleware;

public class AuthorizationMiddleware : IMiddleware
{
    private readonly AppDbContext _dbContext;

    public AuthorizationMiddleware(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var endpoint = context.GetEndpoint();

        // Check for AllowAnonymous attribute
        if (endpoint?.Metadata.GetMetadata<AllowAnonymousAttribute>() != null)
        {
            await next(context);
            return;
        }

        var authorizationPolicy = endpoint?.Metadata.GetMetadata<AuthorizationPolicyMetadata>();

        if (authorizationPolicy != null)
        {
            var user = context.User;
            if (!user.Identity.IsAuthenticated)
            {
                context.Response.StatusCode = 401;
                return;
            }

            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                context.Response.StatusCode = 401;
                return;
            }

            var dbUser = await _dbContext.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .ThenInclude(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(u => u.Id == int.Parse(userId));

            if (dbUser == null || !dbUser.IsActive)
            {
                context.Response.StatusCode = 403;
                return;
            }

            var requiredRoles = authorizationPolicy.Roles;
            var requiredPermissions = authorizationPolicy.Permissions;

            var userRoles = dbUser.UserRoles.Select(ur => ur.Role.Name).ToList();
            var userPermissions = dbUser.UserRoles
                .SelectMany(ur => ur.Role.RolePermissions)
                .Select(rp => rp.Permission.Name)
                .Distinct()
                .ToList();

            bool hasRequiredRole = requiredRoles.Count == 0 || requiredRoles.Any(role => userRoles.Contains(role));
            bool hasRequiredPermission = requiredPermissions.Count == 0 || requiredPermissions.Any(permission => userPermissions.Contains(permission));

            if (!hasRequiredRole || !hasRequiredPermission)
            {
                context.Response.StatusCode = 403;
                return;
            }
        }

        await next(context);
    }
}
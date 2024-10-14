using FastEndpoints;

using FastEndpointsRBAC.Data;
using FastEndpointsRBAC.Models;
using FastEndpointsRBAC.Requests;

using Microsoft.EntityFrameworkCore;

namespace FastEndpointsRBAC.Endpoints.Users;

public class AssignRoleEndpoint : Endpoint<AssignRoleRequest>
{
    private readonly AppDbContext _dbContext;

    public AssignRoleEndpoint(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Post("/users/assign-role");
        Roles("Admin");
    }

    public override async Task HandleAsync(AssignRoleRequest req, CancellationToken ct)
    {
        var user = await _dbContext.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == req.UserId, ct);

        var role = await _dbContext.Roles
            .FirstOrDefaultAsync(r => r.Name == req.RoleName, ct);

        if (user == null || role == null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        if (!user.UserRoles.Any(ur => ur.RoleId == role.Id))
        {
            user.UserRoles.Add(new UserRole { RoleId = role.Id });
            await _dbContext.SaveChangesAsync(ct);
        }

        await SendOkAsync(ct);
    }
}
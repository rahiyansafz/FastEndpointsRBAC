using FastEndpoints;

using FastEndpointsRBAC.Data;
using FastEndpointsRBAC.Models;
using FastEndpointsRBAC.Requests;

using Microsoft.EntityFrameworkCore;

namespace FastEndpointsRBAC.Endpoints.Users;

public class AssignPermissionEndpoint : Endpoint<AssignPermissionRequest>
{
    private readonly AppDbContext _dbContext;

    public AssignPermissionEndpoint(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Post("/roles/assign-permission");
        Roles("Admin");
    }

    public override async Task HandleAsync(AssignPermissionRequest req, CancellationToken ct)
    {
        var role = await _dbContext.Roles
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Name == req.RoleName, ct);

        if (role == null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        var permission = await _dbContext.Permissions
            .FirstOrDefaultAsync(p => p.Name == req.PermissionName, ct);

        if (permission == null)
        {
            permission = new Permission { Name = req.PermissionName };
            _dbContext.Permissions.Add(permission);
        }

        if (!role.RolePermissions.Any(rp => rp.Permission.Name == req.PermissionName))
        {
            role.RolePermissions.Add(new RolePermission { Permission = permission });
            await _dbContext.SaveChangesAsync(ct);
        }

        await SendOkAsync(ct);
    }
}
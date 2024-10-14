using FastEndpoints;

using FastEndpointsRBAC.Data;
using FastEndpointsRBAC.Models;
using FastEndpointsRBAC.Requests;

using Microsoft.EntityFrameworkCore;

namespace FastEndpointsRBAC.Endpoints.Users;

public class CreateRoleEndpoint : Endpoint<CreateRoleRequest>
{
    private readonly AppDbContext _dbContext;

    public CreateRoleEndpoint(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Post("/roles");
        Roles("Admin");
    }

    public override async Task HandleAsync(CreateRoleRequest req, CancellationToken ct)
    {
        var role = new Role
        {
            Name = req.Name
        };

        var permissions = await _dbContext.Permissions
            .Where(p => req.Permissions.Contains(p.Name))
            .ToListAsync(ct);

        role.RolePermissions = permissions.Select(p => new RolePermission { Permission = p }).ToList();

        _dbContext.Roles.Add(role);
        await _dbContext.SaveChangesAsync(ct);

        await SendAsync(new { role.Id, role.Name }, 201, ct);
    }
}
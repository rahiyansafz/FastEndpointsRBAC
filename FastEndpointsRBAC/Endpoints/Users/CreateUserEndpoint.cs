using FastEndpoints;

using FastEndpointsRBAC.Data;
using FastEndpointsRBAC.Models;
using FastEndpointsRBAC.Requests;

using Microsoft.EntityFrameworkCore;

namespace FastEndpointsRBAC.Endpoints.Users;

public class CreateUserEndpoint : Endpoint<CreateUserRequest>
{
    private readonly AppDbContext _dbContext;

    public CreateUserEndpoint(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Post("/users");
        Roles("Admin");
    }

    public override async Task HandleAsync(CreateUserRequest req, CancellationToken ct)
    {
        var user = new User
        {
            Username = req.Username,
            Email = req.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password)
        };

        var roles = await _dbContext.Roles
            .Where(r => req.Roles.Contains(r.Name))
            .ToListAsync(ct);

        user.UserRoles = roles.Select(r => new UserRole { Role = r }).ToList();

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(ct);

        await SendAsync(new { user.Id, user.Username, user.Email }, 201, ct);
    }
}
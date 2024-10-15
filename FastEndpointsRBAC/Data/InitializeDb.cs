using FastEndpointsRBAC.Models;

using Microsoft.EntityFrameworkCore;

namespace FastEndpointsRBAC.Data;

public static class InitializeDb
{
    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        Console.WriteLine("Initializing database...");
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Create roles if they don't exist
        if (!await dbContext.Roles.AnyAsync())
        {
            var adminRole = new Role { Name = "Admin" };
            var userRole = new Role { Name = "User" };

            dbContext.Roles.AddRange(adminRole, userRole);
            await dbContext.SaveChangesAsync();

            // Create basic permissions
            var permissions = new List<Permission>
            {
                new() { Name = "CreateUser" },
                new() { Name = "EditUser" },
                new() { Name = "DeleteUser" },
                new() { Name = "AssignRole" },
                new() { Name = "CreateRole" },
                new() { Name = "EditRole" },
                new() { Name = "DeleteRole" },
                new() { Name = "AssignPermission" }
            };

            dbContext.Permissions.AddRange(permissions);
            await dbContext.SaveChangesAsync();

            // Assign all permissions to Admin role
            foreach (var permission in permissions)
            {
                dbContext.RolePermissions.Add(new RolePermission
                {
                    RoleId = adminRole.Id,
                    PermissionId = permission.Id
                });
            }

            await dbContext.SaveChangesAsync();
        }

        // Create admin user if it doesn't exist
        if (!await dbContext.Users.AnyAsync(u => u.Username == "admin"))
        {
            var adminRole = await dbContext.Roles.FirstAsync(r => r.Name == "Admin");
            var adminUser = new User
            {
                Username = "admin",
                Email = "admin@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("adminpassword"),
                IsActive = true
            };

            dbContext.Users.Add(adminUser);
            await dbContext.SaveChangesAsync();

            // Assign Admin role to admin user
            dbContext.UserRoles.Add(new UserRole
            {
                UserId = adminUser.Id,
                RoleId = adminRole.Id
            });

            await dbContext.SaveChangesAsync();
        }
    }
}
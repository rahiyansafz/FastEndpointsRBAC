using System.Security.Claims;

using FastEndpoints;

using FastEndpointsRBAC.Data;

using Microsoft.EntityFrameworkCore;

namespace FastEndpointsRBAC.Endpoints.Todos;

public class DeleteTodoEndpoint : EndpointWithoutRequest
{
    private readonly AppDbContext _dbContext;

    public DeleteTodoEndpoint(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Delete("/todos/{id}");
        Roles("User", "Admin");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<int>("id");
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

        var todo = await _dbContext.Todos
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId, ct);

        if (todo == null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        _dbContext.Todos.Remove(todo);
        await _dbContext.SaveChangesAsync(ct);

        await SendNoContentAsync(ct);
    }
}
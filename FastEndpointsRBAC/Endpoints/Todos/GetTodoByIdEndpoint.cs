using System.Security.Claims;

using FastEndpoints;

using FastEndpointsRBAC.Data;
using FastEndpointsRBAC.Responses;

using Microsoft.EntityFrameworkCore;

namespace FastEndpointsRBAC.Endpoints.Todos;

public class GetTodoByIdEndpoint : EndpointWithoutRequest<TodoResponse>
{
    private readonly AppDbContext _dbContext;

    public GetTodoByIdEndpoint(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get("/todos/{id}");
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

        var response = new TodoResponse
        {
            Id = todo.Id,
            Title = todo.Title,
            IsCompleted = todo.IsCompleted,
            UserId = todo.UserId
        };

        await SendAsync(response, cancellation: ct);
    }
}
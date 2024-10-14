using System.Security.Claims;

using FastEndpoints;

using FastEndpointsRBAC.Data;
using FastEndpointsRBAC.Responses;

using Microsoft.EntityFrameworkCore;

namespace FastEndpointsRBAC.Endpoints.Todos;

public class GetTodosEndpoint : EndpointWithoutRequest<List<TodoResponse>>
{
    private readonly AppDbContext _dbContext;

    public GetTodosEndpoint(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get("/todos");
        Roles("User", "Admin");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

        var todos = await _dbContext.Todos
            .Where(t => t.UserId == userId)
            .Select(t => new TodoResponse
            {
                Id = t.Id,
                Title = t.Title,
                IsCompleted = t.IsCompleted,
                UserId = t.UserId
            })
            .ToListAsync(ct);

        await SendAsync(todos, cancellation: ct);
    }
}
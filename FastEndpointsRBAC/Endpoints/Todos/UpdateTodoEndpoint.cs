using System.Security.Claims;

using FastEndpoints;

using FastEndpointsRBAC.Data;
using FastEndpointsRBAC.Requests;
using FastEndpointsRBAC.Responses;

using Microsoft.EntityFrameworkCore;

namespace FastEndpointsRBAC.Endpoints.Todos;

public class UpdateTodoEndpoint : Endpoint<UpdateTodoRequest, TodoResponse>
{
    private readonly AppDbContext _dbContext;

    public UpdateTodoEndpoint(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Put("/todos/{id}");
        Roles("User", "Admin");
    }

    public override async Task HandleAsync(UpdateTodoRequest req, CancellationToken ct)
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

        todo.Title = req.Title;
        todo.IsCompleted = req.IsCompleted;

        await _dbContext.SaveChangesAsync(ct);

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

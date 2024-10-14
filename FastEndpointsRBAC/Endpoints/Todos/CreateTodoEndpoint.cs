using System.Security.Claims;

using FastEndpoints;

using FastEndpointsRBAC.Data;
using FastEndpointsRBAC.Models;
using FastEndpointsRBAC.Requests;
using FastEndpointsRBAC.Responses;

namespace FastEndpointsRBAC.Endpoints.Todos;

public class CreateTodoEndpoint : Endpoint<CreateTodoRequest, TodoResponse>
{
    private readonly AppDbContext _dbContext;

    public CreateTodoEndpoint(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Post("/todos");
        Roles("User", "Admin");
    }

    public override async Task HandleAsync(CreateTodoRequest req, CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

        var todo = new Todo
        {
            Title = req.Title,
            IsCompleted = false,
            UserId = userId
        };

        _dbContext.Todos.Add(todo);
        await _dbContext.SaveChangesAsync(ct);

        await SendAsync(new TodoResponse
        {
            Id = todo.Id,
            Title = todo.Title,
            IsCompleted = todo.IsCompleted,
            UserId = todo.UserId
        }, 201, ct);
    }
}
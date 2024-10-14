using FastEndpoints;

using FastEndpointsRBAC.Data;
using FastEndpointsRBAC.Requests;

namespace FastEndpointsRBAC.Endpoints.Users;

public class BlockUserEndpoint : Endpoint<BlockUserRequest>
{
    private readonly AppDbContext _dbContext;

    public BlockUserEndpoint(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Post("/users/block");
        Roles("Admin");
    }

    public override async Task HandleAsync(BlockUserRequest req, CancellationToken ct)
    {
        var user = await _dbContext.Users.FindAsync(new object[] { req.UserId }, ct);

        if (user == null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        user.IsActive = !req.Block;
        await _dbContext.SaveChangesAsync(ct);

        await SendOkAsync(ct);
    }
}
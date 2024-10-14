using FastEndpoints;

using FastEndpointsRBAC.MetaData;

namespace FastEndpointsRBAC.Endpoints;

public class TestEndpoint : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/test");
        //AllowAnonymous();
        Options(x => x.WithMetadata(new AuthorizationPolicyMetadata("Role:Admin", "Permission:CanDoSomething")));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await SendAsync("Test endpoint working!", cancellation: ct);
    }
}
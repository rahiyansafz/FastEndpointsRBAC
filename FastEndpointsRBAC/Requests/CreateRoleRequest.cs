namespace FastEndpointsRBAC.Requests;

public class CreateRoleRequest
{
    public string Name { get; set; }
    public List<string> Permissions { get; set; }
}
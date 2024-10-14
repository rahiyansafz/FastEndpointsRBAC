namespace FastEndpointsRBAC.Requests;

public class AssignRoleRequest
{
    public int UserId { get; set; }
    public string RoleName { get; set; }
}
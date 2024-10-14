namespace FastEndpointsRBAC.Requests;

public class AssignPermissionRequest
{
    public string RoleName { get; set; }
    public string PermissionName { get; set; }
}
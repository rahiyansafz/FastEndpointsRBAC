namespace FastEndpointsRBAC.Requests;

public class CreateUserRequest
{
    public string Username { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public List<string> Roles { get; set; }
}
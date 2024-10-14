namespace FastEndpointsRBAC.Requests;

public class BlockUserRequest
{
    public int UserId { get; set; }
    public bool Block { get; set; }
}

namespace FastEndpointsRBAC.Responses;

public class TodoResponse
{
    public int Id { get; set; }
    public string Title { get; set; }
    public bool IsCompleted { get; set; }
    public int UserId { get; set; }
}
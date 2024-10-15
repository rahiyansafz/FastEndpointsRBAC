namespace FastEndpointsRBAC.MetaData;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class AuthorizationPolicyMetadata : Attribute
{
    public List<string> Roles { get; } = [];
    public List<string> Permissions { get; } = [];

    public AuthorizationPolicyMetadata(params string[] policies)
    {
        foreach (var policy in policies)
        {
            if (policy.StartsWith("Role:"))
            {
                Roles.Add(policy[5..]);
            }
            else if (policy.StartsWith("Permission:"))
            {
                Permissions.Add(policy[11..]);
            }
        }
    }
}
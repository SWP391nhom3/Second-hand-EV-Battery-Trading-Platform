using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Security.Claims;

namespace EVehicle.API.Authorization;

/// <summary>
/// Custom Authorization Policy Provider để tạo policy động cho multiple roles
/// </summary>
public class RoleBasedPolicyProvider : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider;

    public RoleBasedPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        _fallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
    }

    public async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        // Nếu policy đã tồn tại, trả về policy đó
        var existingPolicy = await _fallbackPolicyProvider.GetPolicyAsync(policyName);
        if (existingPolicy != null)
        {
            return existingPolicy;
        }

        // Nếu policy name chứa dấu "_", đó là policy cho multiple roles
        if (policyName.Contains("_"))
        {
            var roles = policyName.Split('_');
            if (roles.Length > 1)
            {
                var allowedRoles = roles.ToHashSet(StringComparer.OrdinalIgnoreCase);
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAssertion(context =>
                    {
                        // Kiểm tra tất cả các claim types có thể chứa role
                        // Tìm trong tất cả claims để đảm bảo tìm được role dù claim type là gì
                        var roleClaim = context.User.Claims
                            .FirstOrDefault(c => 
                                (c.Type == "role" || 
                                 c.Type == ClaimTypes.Role ||
                                 c.Type.EndsWith("/role", StringComparison.OrdinalIgnoreCase)) &&
                                !string.IsNullOrEmpty(c.Value));
                        
                        var userRole = roleClaim?.Value;
                        
                        // Kiểm tra authorization
                        return !string.IsNullOrEmpty(userRole) && allowedRoles.Contains(userRole);
                    })
                    .Build();
                return policy;
            }
        }

        // Fallback về default policy provider
        return await _fallbackPolicyProvider.GetPolicyAsync(policyName);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
    {
        return _fallbackPolicyProvider.GetDefaultPolicyAsync();
    }

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
    {
        return _fallbackPolicyProvider.GetFallbackPolicyAsync();
    }
}


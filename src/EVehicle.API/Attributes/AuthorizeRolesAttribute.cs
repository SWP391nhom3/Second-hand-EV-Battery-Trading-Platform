using Microsoft.AspNetCore.Authorization;
using System.Linq;

namespace EVehicle.API.Attributes;

/// <summary>
/// Attribute để xác định các roles được phép truy cập endpoint
/// Hỗ trợ một hoặc nhiều roles
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class AuthorizeRolesAttribute : AuthorizeAttribute
{
    /// <summary>
    /// Constructor với một role
    /// </summary>
    /// <param name="role">Role được phép (MEMBER, STAFF, ADMIN)</param>
    public AuthorizeRolesAttribute(string role) : base()
    {
        // Với một role, sử dụng Policy đã được cấu hình trong Program.cs
        Policy = role;
    }

    /// <summary>
    /// Constructor với nhiều roles
    /// </summary>
    /// <param name="roles">Danh sách roles được phép (MEMBER, STAFF, ADMIN)</param>
    public AuthorizeRolesAttribute(params string[] roles) : base()
    {
        if (roles == null || roles.Length == 0)
        {
            throw new ArgumentException("Ít nhất một role phải được chỉ định", nameof(roles));
        }

        // Luôn sử dụng Policy, không sử dụng Roles property
        // Vì Roles property sử dụng IsInRole() và có thể không hoạt động đúng với custom RoleClaimType
        if (roles.Length == 1)
        {
            // Chỉ có một role, sử dụng Policy đơn giản
            Policy = roles[0];
        }
        else
        {
            // Với nhiều roles, tạo policy name từ sorted roles
            // Policy sẽ được tạo trong Program.cs với RequireAssertion
            var policyName = string.Join("_", roles.OrderBy(r => r));
            Policy = policyName;
        }
    }
}


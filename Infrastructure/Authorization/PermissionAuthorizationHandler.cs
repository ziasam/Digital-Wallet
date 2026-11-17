using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace DigitalWalletService.Infrastructure.Authorization;

/// <summary>
/// Succeeds when the user has a claim of type "permission" matching the required code.
/// Admins with role claim "Admin" are granted all permissions as a convenience.
/// </summary>
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    public const string PermissionClaimType = "permission";
    public const string RoleClaimType = ClaimTypes.Role;

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.User?.Identity?.IsAuthenticated != true)
            return Task.CompletedTask;

        if (context.User.IsInRole("Admin") ||
            context.User.HasClaim(c =>
                (c.Type == ClaimTypes.Role) &&
                string.Equals(c.Value, "Admin", StringComparison.OrdinalIgnoreCase)))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var hasPermission = context.User.Claims.Any(c =>
            c.Type == PermissionClaimType &&
            string.Equals(c.Value, requirement.Permission, StringComparison.OrdinalIgnoreCase));

        if (hasPermission)
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}

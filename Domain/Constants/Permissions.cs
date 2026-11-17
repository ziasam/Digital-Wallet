namespace DigitalWalletService.Domain.Constants;

/// <summary>
/// Canonical permission codes used in claims and policies.
/// Format: resource.action
/// </summary>
public static class Permissions
{
    // Users
    public const string UsersRead = "users.read";
    public const string UsersCreate = "users.create";
    public const string UsersUpdate = "users.update";
    public const string UsersDelete = "users.delete";
    public const string UsersManageRoles = "users.manage_roles";

    // Roles & permissions
    public const string RolesRead = "roles.read";
    public const string RolesManage = "roles.manage";
    public const string PermissionsRead = "permissions.read";
    public const string PermissionsManage = "permissions.manage";

    // Auth / tokens
    public const string TokensRevoke = "tokens.revoke";
    public const string AuditRead = "audit.read";

    public static readonly string[] All =
    [
        UsersRead, UsersCreate, UsersUpdate, UsersDelete, UsersManageRoles,
        RolesRead, RolesManage,
        PermissionsRead, PermissionsManage,
        TokensRevoke, AuditRead
    ];
}

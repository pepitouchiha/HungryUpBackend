using System.Security.Claims;
using HungryUp.Application.Auth;
using Microsoft.AspNetCore.Authorization;

namespace HungryUp.Api.Authorization;

/// <summary>
/// Resuelve los permisos del usuario a partir de su claim de rol y concede el acceso
/// si el permiso requerido está incluido. No inflamos el JWT con la lista de permisos:
/// el rol viaja en el token y el mapeo rol→permisos se evalúa en cada request.
/// </summary>
public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var rol = context.User.FindFirstValue(ClaimTypes.Role);

        if (RolePermissions.HasPermission(rol, requirement.Permission))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}

using Microsoft.AspNetCore.Authorization;

namespace HungryUp.Api.Authorization;

/// <summary>
/// Exige que el usuario autenticado tenga el permiso indicado (según su rol).
/// Uso: <c>[HasPermission(Permissions.Products.Create)]</c> sobre una acción o controller.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class HasPermissionAttribute : AuthorizeAttribute
{
    public HasPermissionAttribute(string permission) => Policy = permission;
}

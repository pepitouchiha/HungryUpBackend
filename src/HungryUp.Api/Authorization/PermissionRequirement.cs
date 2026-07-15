using Microsoft.AspNetCore.Authorization;

namespace HungryUp.Api.Authorization;

/// <summary>Requisito de autorización que representa un permiso concreto (ej. "products:create").</summary>
public sealed class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }

    public PermissionRequirement(string permission) => Permission = permission;
}

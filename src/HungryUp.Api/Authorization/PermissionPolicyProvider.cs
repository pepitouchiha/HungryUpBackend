using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace HungryUp.Api.Authorization;

/// <summary>
/// Crea políticas de autorización al vuelo: cualquier nombre de política que no exista se interpreta
/// como un permiso y se traduce en un <see cref="PermissionRequirement"/>. Así no hay que registrar
/// una política por cada permiso; basta con declararlo en <c>Permissions</c> y usarlo en el atributo.
/// </summary>
public sealed class PermissionPolicyProvider : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallback;

    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options) =>
        _fallback = new DefaultAuthorizationPolicyProvider(options);

    public async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        // Respeta políticas con nombre ya registradas (si las hubiera).
        var existente = await _fallback.GetPolicyAsync(policyName);
        if (existente is not null)
            return existente;

        return new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddRequirements(new PermissionRequirement(policyName))
            .Build();
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _fallback.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => _fallback.GetFallbackPolicyAsync();
}

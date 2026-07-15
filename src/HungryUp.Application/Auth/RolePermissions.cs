using HungryUp.Domain.Auth;

namespace HungryUp.Application.Auth;

/// <summary>
/// Mapeo rol → permisos. Es el único lugar que decide qué puede hacer cada rol.
/// Hoy es estático en código; para hacerlo administrable en runtime basta con reemplazar
/// la fuente de este diccionario por una consulta a base de datos (la firma pública no cambia).
/// </summary>
public static class RolePermissions
{
    private static readonly IReadOnlySet<string> Ninguno = new HashSet<string>();

    private static readonly IReadOnlyDictionary<RolUsuario, IReadOnlySet<string>> Map =
        new Dictionary<RolUsuario, IReadOnlySet<string>>
        {
            // El administrador siempre tiene todos los permisos declarados.
            [RolUsuario.Admin] = Permissions.All.ToHashSet(),

            // Cajero: opera pedidos, cobra y consulta catálogo/compras/analítica.
            [RolUsuario.Cashier] = new HashSet<string>
            {
                Permissions.Products.Read,
                Permissions.Categories.Read,
                Permissions.Orders.Read,
                Permissions.Orders.Create,
                Permissions.Orders.UpdateStatus,
                Permissions.Mesas.Read,
                Permissions.Mesas.Update,
                Permissions.Billing.Pay,
                Permissions.Analytics.Read,
                Permissions.Purchasing.Read,
            },

            // Mesero: toma pedidos y gestiona el estado de las mesas.
            [RolUsuario.Waiter] = new HashSet<string>
            {
                Permissions.Products.Read,
                Permissions.Categories.Read,
                Permissions.Orders.Read,
                Permissions.Orders.Create,
                Permissions.Orders.UpdateStatus,
                Permissions.Mesas.Read,
                Permissions.Mesas.Update,
            },
        };

    public static IReadOnlySet<string> ForRole(RolUsuario rol) =>
        Map.TryGetValue(rol, out var permisos) ? permisos : Ninguno;

    /// <summary>Resuelve los permisos a partir del nombre del rol (como viene en el claim del JWT).</summary>
    public static IReadOnlySet<string> ForRole(string? rolName) =>
        Enum.TryParse<RolUsuario>(rolName, ignoreCase: true, out var rol) ? ForRole(rol) : Ninguno;

    public static bool HasPermission(string? rolName, string permission) =>
        ForRole(rolName).Contains(permission);
}

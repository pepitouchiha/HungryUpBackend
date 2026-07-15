using System.Reflection;

namespace HungryUp.Application.Auth;

/// <summary>
/// Catálogo central de permisos del sistema (formato "recurso:acción").
/// Agregar un permiso aquí y asignarlo en <see cref="RolePermissions"/> es todo lo necesario para
/// proteger un endpoint con <c>[HasPermission(...)]</c>; no se cablea ningún rol en los controllers.
/// </summary>
public static class Permissions
{
    public static class Products
    {
        public const string Read = "products:read";
        public const string Create = "products:create";
        public const string Update = "products:update";
        public const string Delete = "products:delete";
        public const string AdjustStock = "products:stock";
        public const string ManageImage = "products:image";
    }

    public static class Categories
    {
        public const string Read = "categories:read";
        public const string Create = "categories:create";
        public const string Update = "categories:update";
        public const string Delete = "categories:delete";
    }

    public static class Orders
    {
        public const string Read = "orders:read";
        public const string Create = "orders:create";
        public const string UpdateStatus = "orders:update-status";
    }

    public static class Mesas
    {
        public const string Read = "mesas:read";
        public const string Create = "mesas:create";
        public const string Update = "mesas:update";
        public const string Delete = "mesas:delete";
    }

    public static class Billing
    {
        public const string Pay = "billing:pay";
    }

    public static class Analytics
    {
        public const string Read = "analytics:read";

        /// <summary>Reporte de ganancias/pérdidas (incluye salarios). Información financiera sensible.</summary>
        public const string ProfitLoss = "analytics:profit-loss";
    }

    public static class Employees
    {
        public const string Read = "employees:read";
        public const string Create = "employees:create";
        public const string Update = "employees:update";
        public const string Delete = "employees:delete";
    }

    public static class Purchasing
    {
        public const string Read = "purchasing:read";
        public const string Create = "purchasing:create";
        public const string Update = "purchasing:update";
        public const string Confirm = "purchasing:confirm";
        public const string Anular = "purchasing:anular";
        public const string Delete = "purchasing:delete";
    }

    public static class Users
    {
        public const string Read = "users:read";
        public const string Create = "users:create";
        public const string Update = "users:update";
        public const string Delete = "users:delete";
    }

    /// <summary>Todos los permisos declarados (recolectados por reflexión sobre las constantes anidadas).</summary>
    public static IReadOnlyList<string> All { get; } = typeof(Permissions)
        .GetNestedTypes(BindingFlags.Public | BindingFlags.Static)
        .SelectMany(t => t.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy))
        .Where(f => f is { IsLiteral: true, IsInitOnly: false } && f.FieldType == typeof(string))
        .Select(f => (string)f.GetRawConstantValue()!)
        .OrderBy(p => p)
        .ToList();
}

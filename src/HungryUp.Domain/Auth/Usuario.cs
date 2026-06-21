namespace HungryUp.Domain.Auth;

public class Usuario
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;

    /// <summary>Hash BCrypt de la contraseña. Nunca se almacena en texto plano.</summary>
    public string PasswordHash { get; set; } = string.Empty;

    public RolUsuario Rol { get; set; }
    public bool Activo { get; set; } = true;

    public int EnterpriseId { get; set; }
    public string EnterpriseName { get; set; } = string.Empty;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}

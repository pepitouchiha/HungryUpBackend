using System.ComponentModel.DataAnnotations.Schema;

namespace HungryUp.Domain.Auth;

public class RefreshToken
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }

    /// <summary>Hash SHA-256 del refresh token. El valor en claro nunca se persiste.</summary>
    public string TokenHash { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RevokedAt { get; set; }

    /// <summary>Id del token que reemplazó a éste (cadena de rotación). Permite detectar reuso.</summary>
    public int? ReplacedByTokenId { get; set; }

    /// <summary>Contexto de auditoría (no se usa para enforcing automático).</summary>
    public string? CreatedByIp { get; set; }
    public string? UserAgent { get; set; }

    public Usuario Usuario { get; set; } = null!;

    [NotMapped]
    public bool Activo => RevokedAt is null && DateTime.UtcNow < ExpiresAt;
}

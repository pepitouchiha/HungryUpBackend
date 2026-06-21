using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using HungryUp.Application.Auth.Dtos;
using HungryUp.Domain.Auth;
using HungryUp.Persistence.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace HungryUp.Application.Auth;

public class AuthService : IAuthService
{
    private readonly AuthDbContext _db;
    private readonly JwtSettings _jwt;

    public AuthService(AuthDbContext db, IOptions<JwtSettings> jwt)
    {
        _db = db;
        _jwt = jwt.Value;
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginRequestDto dto, string? ip, string? userAgent)
    {
        var user = await _db.Usuarios
            .FirstOrDefaultAsync(u => u.Username == dto.Username && u.Activo);

        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return null;

        var refresh = await CrearRefreshTokenAsync(user.Id, ip, userAgent);
        return ConstruirRespuesta(user, refresh);
    }

    public async Task<AuthResponseDto?> RefreshAsync(string refreshToken, string? ip, string? userAgent)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return null;

        var hash = HashToken(refreshToken);
        var stored = await _db.RefreshTokens
            .Include(r => r.Usuario)
            .FirstOrDefaultAsync(r => r.TokenHash == hash);

        // Token desconocido: nada que hacer.
        if (stored is null)
            return null;

        // Detección de reuso: el token ya fue revocado/rotado pero alguien lo vuelve a presentar.
        // Se asume compromiso -> se revoca toda la familia de tokens activos del usuario.
        if (stored.RevokedAt is not null)
        {
            await RevocarTodosAsync(stored.UsuarioId);
            return null;
        }

        if (DateTime.UtcNow >= stored.ExpiresAt || !stored.Usuario.Activo)
            return null;

        // Rotación estricta: se emite un refresh nuevo y se invalida el actual enlazándolo.
        var nuevo = await CrearRefreshTokenAsync(stored.UsuarioId, ip, userAgent);
        stored.RevokedAt = DateTime.UtcNow;
        stored.ReplacedByTokenId = nuevo.Token.Id;
        await _db.SaveChangesAsync();

        return ConstruirRespuesta(stored.Usuario, nuevo);
    }

    public async Task LogoutAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return;

        var hash = HashToken(refreshToken);
        var stored = await _db.RefreshTokens
            .FirstOrDefaultAsync(r => r.TokenHash == hash && r.RevokedAt == null);

        if (stored is null)
            return;

        stored.RevokedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    // --- Helpers ---

    private async Task<(RefreshToken Token, string PlainText, DateTime Expiration)> CrearRefreshTokenAsync(
        int usuarioId, string? ip, string? userAgent)
    {
        var plainText = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var expiration = DateTime.UtcNow.AddDays(_jwt.RefreshTokenDays);

        var entity = new RefreshToken
        {
            UsuarioId = usuarioId,
            TokenHash = HashToken(plainText),
            ExpiresAt = expiration,
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = ip,
            UserAgent = userAgent
        };

        _db.RefreshTokens.Add(entity);
        await _db.SaveChangesAsync();

        return (entity, plainText, expiration);
    }

    private async Task RevocarTodosAsync(int usuarioId)
    {
        var activos = await _db.RefreshTokens
            .Where(r => r.UsuarioId == usuarioId && r.RevokedAt == null)
            .ToListAsync();

        foreach (var t in activos)
            t.RevokedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    private AuthResponseDto ConstruirRespuesta(
        Usuario user, (RefreshToken Token, string PlainText, DateTime Expiration) refresh)
    {
        var (accessToken, accessExp) = GenerarAccessToken(user);

        return new AuthResponseDto(
            user.Id,
            user.Username,
            user.Email,
            user.FullName,
            user.Rol.ToString(),
            user.EnterpriseId,
            user.EnterpriseName,
            accessToken,
            accessExp.ToString("O"),
            refresh.PlainText,
            refresh.Expiration.ToString("O"));
    }

    private (string Token, DateTime Expiration) GenerarAccessToken(Usuario u)
    {
        var expiration = DateTime.UtcNow.AddMinutes(_jwt.AccessTokenMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, u.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, u.Id.ToString()),
            new Claim(ClaimTypes.Name, u.Username),
            new Claim(ClaimTypes.Email, u.Email),
            new Claim(ClaimTypes.Role, u.Rol.ToString()),
            new Claim("enterpriseId", u.EnterpriseId.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: expiration,
            signingCredentials: creds);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiration);
    }

    private static string HashToken(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}

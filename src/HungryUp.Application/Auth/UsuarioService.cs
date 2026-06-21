using HungryUp.Application.Auth.Dtos;
using HungryUp.Domain.Auth;
using HungryUp.Persistence.Auth;
using Microsoft.EntityFrameworkCore;

namespace HungryUp.Application.Auth;

public class UsuarioService : IUsuarioService
{
    private readonly AuthDbContext _db;

    public UsuarioService(AuthDbContext db) => _db = db;

    public Task<List<UsuarioDto>> GetAllAsync() =>
        _db.Usuarios
            .OrderBy(u => u.Id)
            .Select(u => new UsuarioDto(u.Id, u.Username, u.Email, u.FullName, u.Rol, u.Activo, u.EnterpriseId, u.EnterpriseName))
            .ToListAsync();

    public async Task<UsuarioDto?> GetByIdAsync(int id)
    {
        var u = await _db.Usuarios.FindAsync(id);
        return u is null ? null : Map(u);
    }

    public async Task<UsuarioDto> CrearAsync(CreateUsuarioDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
            throw new ArgumentException("Username y password son obligatorios.");

        var existe = await _db.Usuarios.AnyAsync(u => u.Username == dto.Username);
        if (existe)
            throw new InvalidOperationException($"El usuario '{dto.Username}' ya existe.");

        var usuario = new Usuario
        {
            Username = dto.Username.Trim(),
            Email = dto.Email.Trim(),
            FullName = dto.FullName.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Rol = dto.Rol,
            Activo = true,
            EnterpriseId = dto.EnterpriseId,
            EnterpriseName = dto.EnterpriseName,
            FechaCreacion = DateTime.UtcNow
        };

        _db.Usuarios.Add(usuario);
        await _db.SaveChangesAsync();
        return Map(usuario);
    }

    public async Task<UsuarioDto> ActualizarAsync(int id, UpdateUsuarioDto dto)
    {
        var usuario = await _db.Usuarios.FindAsync(id)
            ?? throw new KeyNotFoundException($"No existe el usuario con id {id}.");

        usuario.Email = dto.Email.Trim();
        usuario.FullName = dto.FullName.Trim();
        usuario.Rol = dto.Rol;
        usuario.Activo = dto.Activo;

        await _db.SaveChangesAsync();
        return Map(usuario);
    }

    public async Task CambiarPasswordAsync(int id, ChangePasswordDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.NewPassword))
            throw new ArgumentException("La nueva contraseña no puede estar vacía.");

        var usuario = await _db.Usuarios.FindAsync(id)
            ?? throw new KeyNotFoundException($"No existe el usuario con id {id}.");

        usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        await _db.SaveChangesAsync();
    }

    private static UsuarioDto Map(Usuario u) =>
        new(u.Id, u.Username, u.Email, u.FullName, u.Rol, u.Activo, u.EnterpriseId, u.EnterpriseName);
}

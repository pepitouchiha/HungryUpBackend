using HungryUp.Domain.Auth;

namespace HungryUp.Application.Auth.Dtos;

public record UsuarioDto(
    int Id,
    string Username,
    string Email,
    string FullName,
    RolUsuario Rol,
    bool Activo,
    int EnterpriseId,
    string EnterpriseName
);

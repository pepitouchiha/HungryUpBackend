using HungryUp.Domain.Auth;

namespace HungryUp.Application.Auth.Dtos;

public record UpdateUsuarioDto(
    string Email,
    string FullName,
    RolUsuario Rol,
    bool Activo
);

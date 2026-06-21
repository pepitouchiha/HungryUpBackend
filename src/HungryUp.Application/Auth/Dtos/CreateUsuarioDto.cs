using HungryUp.Domain.Auth;

namespace HungryUp.Application.Auth.Dtos;

public record CreateUsuarioDto(
    string Username,
    string Password,
    string Email,
    string FullName,
    RolUsuario Rol,
    int EnterpriseId = 1,
    string EnterpriseName = "HungryUp Restaurant"
);

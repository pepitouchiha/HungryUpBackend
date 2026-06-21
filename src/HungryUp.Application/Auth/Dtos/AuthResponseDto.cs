namespace HungryUp.Application.Auth.Dtos;

/// <summary>Respuesta de login y refresh: par de tokens + datos del usuario.</summary>
public record AuthResponseDto(
    int Id,
    string Username,
    string Email,
    string FullName,
    string Role,
    int EnterpriseId,
    string EnterpriseName,
    string AccessToken,
    string AccessTokenExpiration,
    string RefreshToken,
    string RefreshTokenExpiration
);

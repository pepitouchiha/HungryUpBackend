namespace HungryUp.Application.Auth.Dtos;

public record UserSessionDto(
    int Id,
    string Username,
    string Email,
    string FullName,
    string Role,
    string Token,
    string TokenExpiration,
    int EnterpriseId,
    string EnterpriseName
);

using HungryUp.Application.Auth.Dtos;

namespace HungryUp.Application.Auth;

public interface IAuthService
{
    Task<AuthResponseDto?> LoginAsync(LoginRequestDto dto, string? ip, string? userAgent);
    Task<AuthResponseDto?> RefreshAsync(string refreshToken, string? ip, string? userAgent);
    Task LogoutAsync(string refreshToken);
}

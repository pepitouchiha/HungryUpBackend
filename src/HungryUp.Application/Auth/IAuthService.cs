using HungryUp.Application.Auth.Dtos;

namespace HungryUp.Application.Auth;

public interface IAuthService
{
    UserSessionDto? Login(LoginRequestDto dto);
}

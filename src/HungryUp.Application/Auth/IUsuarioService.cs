using HungryUp.Application.Auth.Dtos;

namespace HungryUp.Application.Auth;

public interface IUsuarioService
{
    Task<List<UsuarioDto>> GetAllAsync();
    Task<UsuarioDto?> GetByIdAsync(int id);
    Task<UsuarioDto> CrearAsync(CreateUsuarioDto dto);
    Task<UsuarioDto> ActualizarAsync(int id, UpdateUsuarioDto dto);
    Task CambiarPasswordAsync(int id, ChangePasswordDto dto);
}

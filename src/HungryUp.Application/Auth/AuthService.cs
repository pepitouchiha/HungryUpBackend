using HungryUp.Application.Auth.Dtos;

namespace HungryUp.Application.Auth;

public class AuthService : IAuthService
{
    private static readonly Dictionary<string, (string Password, UserSessionDto Profile)> _users = new()
    {
        ["admin"] = ("admin123", new(1, "admin",  "admin@hungryup.com",  "Administrador", "Admin",   "", "", 1, "HungryUp Restaurant")),
        ["cajero"] = ("cajero123", new(2, "cajero", "cajero@hungryup.com", "Cajero",        "Cashier", "", "", 1, "HungryUp Restaurant")),
        ["mesero"] = ("mesero123", new(3, "mesero", "mesero@hungryup.com", "Mesero",        "Waiter",  "", "", 1, "HungryUp Restaurant")),
    };

    public UserSessionDto? Login(LoginRequestDto dto)
    {
        if (!_users.TryGetValue(dto.Username, out var entry) || entry.Password != dto.Password)
            return null;

        var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        var expiration = DateTime.UtcNow.AddHours(8).ToString("O");
        return entry.Profile with { Token = token, TokenExpiration = expiration };
    }
}

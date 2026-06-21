namespace HungryUp.Application.Auth;

public class JwtSettings
{
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = "HungryUp";
    public string Audience { get; set; } = "HungryUpClient";

    /// <summary>Duración del access token (corta, en minutos).</summary>
    public int AccessTokenMinutes { get; set; } = 15;

    /// <summary>Duración del refresh token (larga, en días).</summary>
    public int RefreshTokenDays { get; set; } = 7;
}

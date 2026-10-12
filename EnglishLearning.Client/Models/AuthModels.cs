namespace EnglishLearning.Client.Models;

public record LoginRequest(string Email, string Password);
public record RegisterRequest(string Nombre, string Email, string Password);
public record VerifyOtpRequest(string Email, string Code);
public record ResendOtpRequest(string Email);

public record AuthResponse(
    string? AccessToken = null,
    string? RefreshToken = null,
    int ExpiresIn = 0,
    bool RequiresOtp = false,
    string? Email = null,
    string? Message = null);

public record RefreshTokenRequest(string AccessToken, string RefreshToken);
public record RevokeTokenRequest(string RefreshToken);
public record UserProfileResponse(int Id, string Nombre, string Correo, string NivelActual);

namespace EnglishLearning.Client.Models;

public record LoginRequest(string Email, string Password);
public record RegisterRequest(string Nombre, string Email, string Password);
public record AuthResponse(string AccessToken, string RefreshToken, int ExpiresIn);
public record RefreshTokenRequest(string AccessToken, string RefreshToken);
public record RevokeTokenRequest(string RefreshToken);
public record UserProfileResponse(int Id, string Nombre, string Correo, string NivelActual);

namespace EnglishLearning.Client.Models;

public record LoginRequest(string Email, string Password);
public record RegisterRequest(string Nombre, string Email, string Password);
public record VerifyOtpRequest(string ChallengeId, string Code);
public record ResendOtpRequest(string ChallengeId);

public record AuthResponse(
    string? AccessToken = null,
    string? RefreshToken = null,
    int ExpiresIn = 0,
    bool RequiresOtp = false,
    string? Email = null,
    string? Message = null,
    string? ChallengeId = null);

public record RefreshTokenRequest(string RefreshToken);
public record RevokeTokenRequest(string RefreshToken);
public record UserProfileResponse(int Id, string Nombre, string Correo, string NivelActual, SkillMetric? Practice = null);

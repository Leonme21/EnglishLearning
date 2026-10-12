namespace EnglishLearning.Domain.Services;

/// <summary>
/// Contrato para la generación y validación de códigos de un solo uso (One-Time Password / 2FA).
/// </summary>
public interface IOtpService
{
    /// <summary>
    /// Genera un código OTP de 6 dígitos numéricos para el correo especificado y lo almacena con expiración.
    /// </summary>
    string GenerateOtp(string email);

    /// <summary>
    /// Valida si el código provisto coincide con el OTP vigente para el correo. Si es válido, lo consume.
    /// </summary>
    bool ValidateOtp(string email, string code);

    /// <summary>
    /// Indica si existe un OTP pendiente y no expirado para el correo.
    /// </summary>
    bool HasPendingOtp(string email);
}

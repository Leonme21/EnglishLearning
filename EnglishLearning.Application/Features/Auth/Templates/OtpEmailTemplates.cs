namespace EnglishLearning.Application.Features.Auth.Templates;

public static class OtpEmailTemplates
{
    public static string BuildOtpEmailHtml(string nombre, string otpCode)
    {
        var safeNombre = string.IsNullOrWhiteSpace(nombre) ? "Estudiante" : System.Net.WebUtility.HtmlEncode(nombre);
        var safeOtp = System.Net.WebUtility.HtmlEncode(otpCode);

        return $@"<!DOCTYPE html>
<html lang=""es"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Código de Verificación</title>
</head>
<body style=""margin: 0; padding: 0; background-color: #0b0c10; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; color: #f0f0f5;"">
    <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" border=""0"" style=""background-color: #0b0c10; padding: 40px 15px;"">
        <tr>
            <td align=""center"">
                <table role=""presentation"" width=""100%"" max-width=""520px"" style=""max-width: 520px; background-color: #141622; border-radius: 16px; border: 1px solid rgba(255, 255, 255, 0.08); box-shadow: 0 10px 40px rgba(0,0,0,0.5); overflow: hidden;"" cellspacing=""0"" cellpadding=""0"" border=""0"">
                    <!-- Header -->
                    <tr>
                        <td style=""background: linear-gradient(135deg, #4f46e5 0%, #7c3aed 100%); padding: 32px 30px; text-align: center;"">
                            <h1 style=""margin: 0; font-size: 26px; font-weight: 800; color: #ffffff; letter-spacing: -0.5px;"">English Learning</h1>
                            <p style=""margin: 6px 0 0 0; font-size: 14px; color: rgba(255,255,255,0.85);"">Seguridad de tu cuenta</p>
                        </td>
                    </tr>
                    <!-- Body Content -->
                    <tr>
                        <td style=""padding: 36px 32px;"">
                            <p style=""margin: 0 0 16px 0; font-size: 16px; color: #e2e8f0; line-height: 1.5;"">
                                Hola <strong style=""color: #ffffff;"">{safeNombre}</strong>,
                            </p>
                            <p style=""margin: 0 0 28px 0; font-size: 15px; color: #94a3b8; line-height: 1.6;"">
                                Se ha solicitado un inicio de sesión en tu cuenta. Para continuar de forma segura, ingresa el siguiente código de verificación en la plataforma:
                            </p>

                            <!-- OTP Box -->
                            <div style=""background: rgba(99, 102, 241, 0.08); border: 2px dashed #6366f1; border-radius: 12px; padding: 24px; text-align: center; margin-bottom: 28px;"">
                                <span style=""display: block; font-size: 13px; text-transform: uppercase; letter-spacing: 2px; color: #818cf8; font-weight: 600; margin-bottom: 8px;"">Tu código OTP</span>
                                <div style=""font-family: 'Courier New', Courier, monospace; font-size: 38px; font-weight: 800; letter-spacing: 10px; color: #ffffff; text-shadow: 0 2px 10px rgba(99, 102, 241, 0.5);"">
                                    {safeOtp}
                                </div>
                            </div>

                            <p style=""margin: 0 0 20px 0; font-size: 13px; color: #f59e0b; line-height: 1.5; background: rgba(245, 158, 11, 0.1); border-left: 3px solid #f59e0b; padding: 10px 14px; border-radius: 4px;"">
                                ⏱️ Este código expira en <strong>5 minutos</strong> y es de un solo uso.
                            </p>

                            <p style=""margin: 0; font-size: 13px; color: #64748b; line-height: 1.5;"">
                                Si no intentaste iniciar sesión, puedes ignorar este correo; tu contraseña sigue estando segura.
                            </p>
                        </td>
                    </tr>
                    <!-- Footer -->
                    <tr>
                        <td style=""background-color: #0d0e16; padding: 20px 30px; text-align: center; border-top: 1px solid rgba(255, 255, 255, 0.05);"">
                            <p style=""margin: 0; font-size: 12px; color: #475569;"">
                                © {DateTime.UtcNow.Year} English Learning Platform. Todos los derechos reservados.
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
    }
}

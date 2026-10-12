using System.Security.Claims;
using EnglishLearning.Application.Abstractions;
using EnglishLearning.Application.Features.Auth.Commands;
using EnglishLearning.Application.Features.Auth.Handlers;
using EnglishLearning.Application.Options;
using EnglishLearning.Domain.Entities;
using EnglishLearning.Domain.Enums;
using EnglishLearning.Domain.Interfaces;
using EnglishLearning.Domain.Services;
using EnglishLearning.Infrastructure.Services;
using EnglishLearning.Tests.Fakes;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace EnglishLearning.Tests.Auth;

public class OtpAuthTests
{
    private class FakeEmailService : IEmailService
    {
        public List<(string To, string Subject, string Body)> SentEmails { get; } = new();

        public Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
        {
            SentEmails.Add((to, subject, body));
            return Task.CompletedTask;
        }
    }

    private class FakeJwtTokenService : IJwtTokenService
    {
        public string GenerateAccessToken(int userId, string email, string nivelActual) => "fake_access_token";
        public string GenerateRefreshToken() => "fake_refresh_token";
        public Task<string> GenerateAndStoreRefreshTokenAsync(int userId, CancellationToken cancellationToken = default)
            => Task.FromResult("fake_refresh_token");
        public Task<bool> ValidateRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
            => Task.FromResult(true);
        public Task<bool> RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
            => Task.FromResult(true);
        public Task<string?> RotateRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
            => Task.FromResult<string?>("fake_rotated_token");
        public ClaimsPrincipal? ValidateToken(string token) => new ClaimsPrincipal();
        public int? GetUserIdFromToken(string token) => 1;
    }

    private class FakePasswordHasher : IPasswordHasher
    {
        public string HashPassword(string password) => "hashed_" + password;
        public bool VerifyPassword(string password, string hash) => hash == "hashed_" + password;
    }

    private class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveChangesCallCount { get; private set; }
        public ILeccionQueryRepository LeccionesQuery => null!;
        public ILeccionCommandRepository LeccionesCommand => null!;
        public IUsuarioQueryRepository UsuariosQuery => null!;
        public IUsuarioCommandRepository UsuariosCommand => null!;
        public IAssessmentQueryRepository AssessmentsQuery => null!;
        public IAssessmentCommandRepository AssessmentsCommand => null!;
        public IRefreshTokenQueryRepository RefreshTokensQuery => null!;
        public IRefreshTokenCommandRepository RefreshTokensCommand => null!;
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesCallCount++;
            return Task.FromResult(1);
        }
        public Task BeginTransactionAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task CommitTransactionAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task RollbackTransactionAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Dispose() { }
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }

    [Fact]
    public void OtpService_Genera_Y_Valida_Codigo_Exitosamente()
    {
        // Arrange
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var otpService = new OtpService(memoryCache, NullLogger<OtpService>.Instance);
        var email = "test@domain.com";

        // Act
        var code = otpService.GenerateOtp(email);

        // Assert
        Assert.NotNull(code);
        Assert.Equal(6, code.Length);
        Assert.True(otpService.HasPendingOtp(email));

        // Validación exitosa
        var isValid = otpService.ValidateOtp(email, code);
        Assert.True(isValid);

        // El código debe ser de un solo uso (consumido)
        var isUsedAgain = otpService.ValidateOtp(email, code);
        Assert.False(isUsedAgain);
    }

    [Fact]
    public void OtpService_CodigoInvalido_O_Vacio_Retorna_False()
    {
        // Arrange
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var otpService = new OtpService(memoryCache, NullLogger<OtpService>.Instance);
        var email = "usuario@domain.com";
        otpService.GenerateOtp(email);

        // Act & Assert
        Assert.False(otpService.ValidateOtp(email, "000000"));
        Assert.False(otpService.ValidateOtp(email, ""));
        Assert.False(otpService.ValidateOtp("", "123456"));
    }

    [Fact]
    public async Task LoginHandler_CredencialesValidas_GeneraOtp_Y_EnviaCorreo()
    {
        // Arrange
        var userQueryRepo = new FakeUsuarioQueryRepository();
        var hasher = new FakePasswordHasher();
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var otpService = new OtpService(memoryCache, NullLogger<OtpService>.Instance);
        var emailService = new FakeEmailService();
        var smtpOptions = Options.Create(new SmtpSettings { FromEmail = "no-reply@domain.com", FromName = "English Learning" });

        var usuario = Usuario.Create(1, "Carlos", "carlos@test.com", hasher.HashPassword("Password123!"), NivelIngles.B1).Value;
        userQueryRepo.AgregarUsuario(usuario);

        var handler = new LoginHandler(
            userQueryRepo,
            hasher,
            otpService,
            emailService,
            smtpOptions,
            NullLogger<LoginHandler>.Instance);

        var command = new LoginCommand("carlos@test.com", "Password123!");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value.RequiresOtp);
        Assert.Null(result.Value.AccessToken);
        Assert.Equal("carlos@test.com", result.Value.Email);
        Assert.Single(emailService.SentEmails);
        Assert.Equal("carlos@test.com", emailService.SentEmails[0].To);
        Assert.Contains("código de verificación", emailService.SentEmails[0].Subject, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task VerifyOtpHandler_CodigoCorrecto_Retorna_TokensJwt()
    {
        // Arrange
        var userQueryRepo = new FakeUsuarioQueryRepository();
        var hasher = new FakePasswordHasher();
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var otpService = new OtpService(memoryCache, NullLogger<OtpService>.Instance);
        var jwtService = new FakeJwtTokenService();
        var uow = new FakeUnitOfWork();

        var usuario = Usuario.Create(1, "Laura", "laura@test.com", hasher.HashPassword("Password123!"), NivelIngles.A2).Value;
        userQueryRepo.AgregarUsuario(usuario);

        var code = otpService.GenerateOtp("laura@test.com");

        var handler = new VerifyOtpHandler(
            userQueryRepo,
            otpService,
            jwtService,
            uow,
            NullLogger<VerifyOtpHandler>.Instance);

        var command = new VerifyOtpCommand("laura@test.com", code);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.Value.RequiresOtp);
        Assert.Equal("fake_access_token", result.Value.AccessToken);
        Assert.Equal("fake_refresh_token", result.Value.RefreshToken);
        Assert.Equal(1, uow.SaveChangesCallCount);
    }

    [Fact]
    public async Task VerifyOtpHandler_CodigoInvalido_Retorna_Failure()
    {
        // Arrange
        var userQueryRepo = new FakeUsuarioQueryRepository();
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var otpService = new OtpService(memoryCache, NullLogger<OtpService>.Instance);
        var jwtService = new FakeJwtTokenService();
        var uow = new FakeUnitOfWork();

        var handler = new VerifyOtpHandler(
            userQueryRepo,
            otpService,
            jwtService,
            uow,
            NullLogger<VerifyOtpHandler>.Instance);

        var command = new VerifyOtpCommand("laura@test.com", "999999");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Auth.InvalidOtp", result.Error.Code);
    }
}

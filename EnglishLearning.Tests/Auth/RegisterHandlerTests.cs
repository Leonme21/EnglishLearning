using System.Security.Claims;
using EnglishLearning.Application.Abstractions;
using EnglishLearning.Application.Features.Auth.Commands;
using EnglishLearning.Application.Features.Auth.Handlers;
using EnglishLearning.Domain.Entities;
using EnglishLearning.Domain.Enums;
using EnglishLearning.Domain.Interfaces;
using EnglishLearning.Domain.Services;
using EnglishLearning.Tests.Fakes;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace EnglishLearning.Tests.Auth;

public class RegisterHandlerTests
{
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
            => Task.FromResult<string?>("fake_rotated_refresh_token");
        public ClaimsPrincipal? ValidateToken(string token) => new ClaimsPrincipal();
        public int? GetUserIdFromToken(string token) => 1;
    }

    private class FakePasswordHasher : IPasswordHasher
    {
        public string HashPassword(string password) => $"hashed_{password}";
        public bool VerifyPassword(string password, string hash) => hash == $"hashed_{password}";
    }

    private class FakeEmailService : IEmailService
    {
        public List<(string To, string Subject, string Body)> SentEmails { get; } = new();

        public Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
        {
            SentEmails.Add((to, subject, body));
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task RegisterHandler_SuccessfulRegistration_ShouldCreateUserAndSendWelcomeEmail()
    {
        // Arrange
        var userQueryRepo = new FakeUsuarioQueryRepository();
        var userCommandRepo = new FakeUsuarioCommandRepository();
        var jwtService = new FakeJwtTokenService();
        var passwordHasher = new FakePasswordHasher();
        var emailService = new FakeEmailService();
        var unitOfWork = new FakeUnitOfWork();

        var handler = new RegisterHandler(
            userQueryRepo,
            userCommandRepo,
            jwtService,
            passwordHasher,
            emailService,
            unitOfWork,
            NullLogger<RegisterHandler>.Instance);

        var command = new RegisterCommand("Juan Perez", "juan@example.com", "Password123!");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("fake_access_token", result.Value.AccessToken);
        Assert.Equal("juan@example.com", result.Value.User.Email);

        // Verify email was sent
        Assert.Single(emailService.SentEmails);
        var sentEmail = emailService.SentEmails[0];
        Assert.Equal("juan@example.com", sentEmail.To);
        Assert.Contains("Bienvenido", sentEmail.Subject);

        // Verify unit of work persisted changes
        Assert.True(unitOfWork.SaveChangesCallCount >= 2);
    }

    [Fact]
    public async Task RegisterHandler_DuplicateEmail_ShouldReturnConflictFailure()
    {
        // Arrange
        var userQueryRepo = new FakeUsuarioQueryRepository();
        var existingUser = Usuario.Create(1, "Existing", "dup@example.com", "hash", NivelIngles.A1).Value;
        userQueryRepo.AgregarUsuario(existingUser);

        var userCommandRepo = new FakeUsuarioCommandRepository();
        var jwtService = new FakeJwtTokenService();
        var passwordHasher = new FakePasswordHasher();
        var emailService = new FakeEmailService();
        var unitOfWork = new FakeUnitOfWork();

        var handler = new RegisterHandler(
            userQueryRepo,
            userCommandRepo,
            jwtService,
            passwordHasher,
            emailService,
            unitOfWork,
            NullLogger<RegisterHandler>.Instance);

        var command = new RegisterCommand("Otro", "dup@example.com", "Pass123!");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Auth.EmailExists", result.Error.Code);
        Assert.Empty(emailService.SentEmails);
    }
}

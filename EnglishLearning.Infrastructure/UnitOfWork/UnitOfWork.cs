using EnglishLearning.Domain.Interfaces;
using EnglishLearning.Infrastructure.Persistence;
using EnglishLearning.Infrastructure.Repositories.Lessons;
using EnglishLearning.Infrastructure.Repositories.Users;
using EnglishLearning.Infrastructure.Repositories.Assessments;
using EnglishLearning.Infrastructure.Repositories.Auth;
using Microsoft.EntityFrameworkCore.Storage;

namespace EnglishLearning.Infrastructure.UnitOfWork;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly EnglishLearningDbContext _context;
    private IDbContextTransaction? _transaction;

    public ILeccionQueryRepository LeccionesQuery { get; }
    public ILeccionCommandRepository LeccionesCommand { get; }
    public IUsuarioQueryRepository UsuariosQuery { get; }
    public IUsuarioCommandRepository UsuariosCommand { get; }
    public IAssessmentQueryRepository AssessmentsQuery { get; }
    public IAssessmentCommandRepository AssessmentsCommand { get; }
    public IRefreshTokenQueryRepository RefreshTokensQuery { get; }
    public IRefreshTokenCommandRepository RefreshTokensCommand { get; }

    public UnitOfWork(
        EnglishLearningDbContext context,
        LeccionQueryRepository leccionQueryRepository,
        LeccionCommandRepository leccionCommandRepository,
        UsuarioQueryRepository usuarioQueryRepository,
        UsuarioCommandRepository usuarioCommandRepository,
        AssessmentQueryRepository assessmentQueryRepository,
        AssessmentCommandRepository assessmentCommandRepository,
        RefreshTokenQueryRepository refreshTokenQueryRepository,
        RefreshTokenCommandRepository refreshTokenCommandRepository)
    {
        _context = context;
        LeccionesQuery = leccionQueryRepository;
        LeccionesCommand = leccionCommandRepository;
        UsuariosQuery = usuarioQueryRepository;
        UsuariosCommand = usuarioCommandRepository;
        AssessmentsQuery = assessmentQueryRepository;
        AssessmentsCommand = assessmentCommandRepository;
        RefreshTokensQuery = refreshTokenQueryRepository;
        RefreshTokensCommand = refreshTokenCommandRepository;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        if (_transaction != null)
            await _transaction.DisposeAsync();
        await _context.DisposeAsync();
    }
}
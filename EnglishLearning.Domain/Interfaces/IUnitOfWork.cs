using EnglishLearning.Domain.Interfaces;

namespace EnglishLearning.Domain.Interfaces;

public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    ILeccionQueryRepository LeccionesQuery { get; }
    ILeccionCommandRepository LeccionesCommand { get; }
    IUsuarioQueryRepository UsuariosQuery { get; }
    IUsuarioCommandRepository UsuariosCommand { get; }
    IAssessmentQueryRepository AssessmentsQuery { get; }
    IAssessmentCommandRepository AssessmentsCommand { get; }
    IRefreshTokenQueryRepository RefreshTokensQuery { get; }
    IRefreshTokenCommandRepository RefreshTokensCommand { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
using EnglishLearning.Domain.Enums;
using EnglishLearning.Domain.ValueObjects;
namespace EnglishLearning.Domain.Events;

public interface IDomainEvent
{
    DateTime OccurredOn { get; }
    Guid EventId { get; }
}

public abstract record DomainEvent : IDomainEvent
{
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public Guid EventId { get; init; } = Guid.NewGuid();
}

public sealed record LevelChangedEvent : DomainEvent
{
    public UserId UserId { get; init; }
    public NivelIngles PreviousLevel { get; init; }
    public NivelIngles NewLevel { get; init; }
    public Percentage AchievementPercentage { get; init; }
}

public sealed record AssessmentCompletedEvent : DomainEvent
{
    public UserId UserId { get; init; }
    public NivelIngles AssignedLevel { get; init; }
    public int TotalQuestions { get; init; }
    public int CorrectAnswers { get; init; }
    public Percentage TotalScore { get; init; }
    public IReadOnlyDictionary<Habilidad, Percentage> ScoresBySkill { get; init; } = new Dictionary<Habilidad, Percentage>();
}

public sealed record ExerciseEvaluatedEvent : DomainEvent
{
    public UserId UserId { get; init; }
    public ExerciseId ExerciseId { get; init; }
    public bool IsCorrect { get; init; }
    public NivelIngles UserLevel { get; init; }
    public TimeSpan ResponseTime { get; init; }
}

public interface IDomainEventDispatcher
{
    Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
    Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}

public interface IDomainEventHandler<in TEvent> where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken = default);
}
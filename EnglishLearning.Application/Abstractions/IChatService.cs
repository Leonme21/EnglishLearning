namespace EnglishLearning.Application.Abstractions;

public interface IChatService
{
    Task<string> SendMessageAsync(string message, CancellationToken cancellationToken = default);
}

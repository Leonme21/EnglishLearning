using EnglishLearning.Client.Models;
using EnglishLearning.Client.Services.Api;

namespace EnglishLearning.Client.Services;

public class ChatService
{
    private readonly ApiClient _apiClient;

    public ChatService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<ApiResult<ChatResponse>> SendMessageAsync(string message, IReadOnlyList<TutorTurn> history)
    {
        return await _apiClient.PostAsync<ChatResponse>("api/v1/chat/ask", new ChatRequest(message, history));
    }

    public Task<ApiResult<TutorRubric>> GetProductionFeedbackAsync(ProductionFeedbackRequest request) =>
        _apiClient.PostAsync<TutorRubric>("api/v1/chat/production-feedback", request);
}

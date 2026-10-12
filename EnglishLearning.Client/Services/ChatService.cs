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

    public async Task<ApiResult<ChatResponse>> SendMessageAsync(string message)
    {
        return await _apiClient.PostAsync<ChatResponse>("api/v1/chat/ask", new ChatRequest(message));
    }
}

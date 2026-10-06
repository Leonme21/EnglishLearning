using EnglishLearning.Client.Models;
using EnglishLearning.Client.Services.Api;

namespace EnglishLearning.Client.Services;

public class ProgressService
{
    private readonly ApiClient _apiClient;

    public ProgressService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<ApiResult<ProgressReportResponse>> GetProgressAsync(int usuarioId)
    {
        return _apiClient.GetAsync<ProgressReportResponse>($"api/v1/assessments/progress/{usuarioId}");
    }
}

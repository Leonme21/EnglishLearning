using EnglishLearning.Client.Models;
using EnglishLearning.Client.Services.Api;

namespace EnglishLearning.Client.Services;

public class AssessmentService
{
    private readonly ApiClient _apiClient;

    public AssessmentService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<ApiResult<List<DiagnosticQuestion>>> GetQuestionsAsync()
    {
        return _apiClient.GetAsync<List<DiagnosticQuestion>>("api/v1/assessments/questions");
    }

    public Task<ApiResult<SubmitAssessmentResponse>> SubmitAsync(SubmitAssessmentRequest request)
    {
        return _apiClient.PostAsync<SubmitAssessmentResponse>("api/v1/assessments/submit", request);
    }
}

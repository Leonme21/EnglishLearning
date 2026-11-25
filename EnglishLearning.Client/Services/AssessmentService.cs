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

    public Task<ApiResult<AssessmentSession>> GetQuestionsAsync()
    {
        return _apiClient.GetAsync<AssessmentSession>("api/v1/assessments/questions");
    }

    public Task<ApiResult<SubmitAssessmentResponse>> SubmitAsync(SubmitAssessmentRequest request)
    {
        return _apiClient.PostAsync<SubmitAssessmentResponse>("api/v1/assessments/submit", request);
    }
}

using EnglishLearning.Client.Models;
using EnglishLearning.Client.Services.Api;

namespace EnglishLearning.Client.Services;

public class LessonService
{
    private readonly ApiClient _apiClient;

    public LessonService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<ApiResult<List<LessonDto>>> GetLessonsByLevelAsync(string nivel)
    {
        return _apiClient.GetAsync<List<LessonDto>>($"api/v1/lessons/by-level/{nivel}");
    }

    public Task<ApiResult<LessonDto>> GetLessonByIdAsync(int id)
    {
        return _apiClient.GetAsync<LessonDto>($"api/v1/lessons/{id}");
    }

    public Task<ApiResult<EvaluateExerciseResponse>> EvaluateAsync(EvaluateExerciseRequest request)
    {
        return _apiClient.PostAsync<EvaluateExerciseResponse>("api/v1/lessons/evaluate", request);
    }
}

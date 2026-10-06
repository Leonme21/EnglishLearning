using System.Net.Http.Json;
using EnglishLearning.Client.Models;

namespace EnglishLearning.Client.Services.Api;

public class ApiClient
{
    private readonly HttpClient _httpClient;

    public ApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ApiResult<T>> GetAsync<T>(string url)
    {
        var response = await _httpClient.GetAsync(url);
        return await ProcessResponse<T>(response);
    }

    public async Task<ApiResult<T>> PostAsync<T>(string url, object? data)
    {
        var response = await _httpClient.PostAsJsonAsync(url, data);
        return await ProcessResponse<T>(response);
    }

    private async Task<ApiResult<T>> ProcessResponse<T>(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<T>>();
            if (result != null && result.Success)
            {
                return ApiResult<T>.Success(result.Data);
            }
        }

        try
        {
            var errorResult = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
            if (errorResult?.Error != null)
            {
                return ApiResult<T>.Failure(errorResult.Error);
            }
        }
        catch { /* ignored */ }

        return ApiResult<T>.Failure(new ApiError("UnknownError", "Ocurrió un error inesperado al conectar con el servidor."));
    }
}

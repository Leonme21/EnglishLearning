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
        return await SendAsync<T>(() => _httpClient.GetAsync(url));
    }

    public async Task<ApiResult<T>> PostAsync<T>(string url, object? data)
    {
        return await SendAsync<T>(() => _httpClient.PostAsJsonAsync(url, data));
    }

    private static async Task<ApiResult<T>> SendAsync<T>(Func<Task<HttpResponseMessage>> send)
    {
        try
        {
            using var response = await send();
            return await ProcessResponse<T>(response);
        }
        catch (HttpRequestException)
        {
            return ApiResult<T>.Failure(new ApiError("NetworkError", "No se pudo conectar con el servidor."));
        }
        catch (System.Text.Json.JsonException)
        {
            return ApiResult<T>.Failure(new ApiError("InvalidResponse", "La respuesta del servidor no es válida."));
        }
        catch (TaskCanceledException)
        {
            return ApiResult<T>.Failure(new ApiError("Timeout", "El servidor tardó demasiado en responder."));
        }
    }

    private static async Task<ApiResult<T>> ProcessResponse<T>(HttpResponseMessage response)
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

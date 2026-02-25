using System.Net.Http.Json;
using System.Text.Json;

namespace MVC.Services;

public class ApiService
{
    private readonly IHttpClientFactory httpClientFactory;
    private readonly ILogger<ApiService> logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve,
        PropertyNameCaseInsensitive = true
    };

    public ApiService(IHttpClientFactory _httpClientFactory, ILogger<ApiService> _logger)
    {
        httpClientFactory = _httpClientFactory;
        logger = _logger;
    }

    private HttpClient GetClient()
    {
        return httpClientFactory.CreateClient("ApiClient");
    }

    public async Task<T?> GetAsync<T>(string endpoint)
    {
        try
        {
            var client = GetClient();
            var response = await client.GetAsync(endpoint);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<T>(JsonOptions);
            }
            
            logger.LogWarning("API GET {Endpoint} returned {StatusCode}", endpoint, response.StatusCode);
            return default;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error calling API GET {Endpoint}", endpoint);
            return default;
        }
    }

    public async Task<T?> PostAsync<T>(string endpoint, object data)
    {
        try
        {
            var client = GetClient();
            var response = await client.PostAsJsonAsync(endpoint, data, JsonOptions);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<T>(JsonOptions);
            }
            
            logger.LogWarning("API POST {Endpoint} returned {StatusCode}", endpoint, response.StatusCode);
            return default;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error calling API POST {Endpoint}", endpoint);
            return default;
        }
    }

    public async Task<bool> PutAsync(string endpoint, object data)
    {
        try
        {
            var client = GetClient();
            var response = await client.PutAsJsonAsync(endpoint, data, JsonOptions);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error calling API PUT {Endpoint}", endpoint);
            return false;
        }
    }

    public async Task<bool> DeleteAsync(string endpoint)
    {
        try
        {
            var client = GetClient();
            var response = await client.DeleteAsync(endpoint);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error calling API DELETE {Endpoint}", endpoint);
            return false;
        }
    }

    public async Task<bool> PostActionAsync(string endpoint)
    {
        try
        {
            var client = GetClient();
            var response = await client.PostAsync(endpoint, null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error calling API POST action {Endpoint}", endpoint);
            return false;
        }
    }

    public async Task<bool> PostActionAsync(string endpoint, object data)
    {
        try
        {
            var client = GetClient();
            var response = await client.PostAsJsonAsync(endpoint, data, JsonOptions);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error calling API POST action {Endpoint}", endpoint);
            return false;
        }
    }
}

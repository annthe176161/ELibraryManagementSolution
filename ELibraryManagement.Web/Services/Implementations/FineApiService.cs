using ELibraryManagement.Web.Models;
using ELibraryManagement.Web.Services.Interfaces;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ELibraryManagement.Web.Services.Implementations
{
    public class FineApiService : IFineApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<FineApiService> _logger;
        private readonly IConfiguration _configuration;
        private readonly JsonSerializerOptions _jsonOptions;

        public FineApiService(HttpClient httpClient, ILogger<FineApiService> logger, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _configuration = configuration;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        private string GetApiBaseUrl() => _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000";

        public void SetAuthToken(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        private void SetAuthorizationHeader(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<(List<FineViewModel> fines, int totalCount, int totalPages)> GetAllFinesAsync(int page = 1, int pageSize = 20, string? status = null, string? search = null)
        {
            try
            {
                var queryParams = new List<string>
                {
                    $"page={page}",
                    $"pageSize={pageSize}"
                };

                if (!string.IsNullOrEmpty(status))
                    queryParams.Add($"status={status}");

                if (!string.IsNullOrEmpty(search))
                    queryParams.Add($"search={Uri.EscapeDataString(search)}");

                var queryString = string.Join("&", queryParams);
                var response = await _httpClient.GetAsync($"{GetApiBaseUrl()}/api/Fines?{queryString}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<FineListResponse>(content, _jsonOptions);

                    return (result?.Fines ?? new List<FineViewModel>(), result?.TotalCount ?? 0, result?.TotalPages ?? 0);
                }

                _logger.LogWarning("Failed to get fines. Status: {StatusCode}", response.StatusCode);
                return (new List<FineViewModel>(), 0, 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all fines");
                return (new List<FineViewModel>(), 0, 0);
            }
        }

        public async Task<FineDetailViewModel?> GetFineDetailsAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{GetApiBaseUrl()}/api/Fines/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<FineDetailViewModel>(content, _jsonOptions);
                }

                _logger.LogWarning("Failed to get fine details for ID: {FineId}. Status: {StatusCode}", id, response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting fine details for ID: {FineId}", id);
                return null;
            }
        }

        public async Task<bool> CreateFineAsync(CreateFineRequest request)
        {
            try
            {
                var json = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{GetApiBaseUrl()}/api/Fines", content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Fine created successfully for user: {UserId}", request.UserId);
                    return true;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Failed to create fine. Status: {StatusCode}, Error: {Error}", response.StatusCode, errorContent);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating fine");
                return false;
            }
        }

        public async Task<bool> UpdateFineAsync(int id, UpdateFineRequest request)
        {
            try
            {
                var json = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"{GetApiBaseUrl()}/api/Fines/{id}", content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Fine updated successfully for ID: {FineId}", id);
                    return true;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Failed to update fine. Status: {StatusCode}, Error: {Error}", response.StatusCode, errorContent);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating fine with ID: {FineId}", id);
                return false;
            }
        }

        public async Task<bool> MarkFineAsPaidAsync(int id, string? notes = null)
        {
            try
            {
                _logger.LogInformation("Starting MarkFineAsPaidAsync for Fine ID: {FineId} with notes: {Notes}", id, notes ?? "none");

                var request = new { notes };
                var json = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var url = $"{GetApiBaseUrl()}/api/Fines/{id}/pay";
                _logger.LogInformation("Calling API endpoint: {Url}", url);

                // Log authorization header
                var authHeader = _httpClient.DefaultRequestHeaders.Authorization;
                _logger.LogInformation("Authorization header: {AuthHeader}", authHeader != null ? $"{authHeader.Scheme} {authHeader.Parameter?[..10]}..." : "None");

                var response = await _httpClient.PostAsync(url, content);

                _logger.LogInformation("API response status: {StatusCode}", response.StatusCode);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("Fine marked as paid successfully for ID: {FineId}. Response: {Response}", id, responseContent);
                    return true;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Failed to mark fine as paid. Status: {StatusCode}, Error: {Error}", response.StatusCode, errorContent);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking fine as paid for ID: {FineId}", id);
                return false;
            }
        }

        public async Task<bool> WaiveFineAsync(int id, string reason, string? notes = null)
        {
            try
            {
                var request = new { reason, notes };
                var json = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{GetApiBaseUrl()}/api/Fines/{id}/waive", content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Fine waived successfully for ID: {FineId}", id);
                    return true;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Failed to waive fine. Status: {StatusCode}, Error: {Error}", response.StatusCode, errorContent);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error waiving fine for ID: {FineId}", id);
                return false;
            }
        }

        public async Task<List<FineViewModel>> GetUserFinesAsync(string userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{GetApiBaseUrl()}/api/Fines/user/{userId}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<FineViewModel>>(content, _jsonOptions) ?? new List<FineViewModel>();
                }

                _logger.LogWarning("Failed to get user fines for user: {UserId}. Status: {StatusCode}", userId, response.StatusCode);
                return new List<FineViewModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user fines for user: {UserId}", userId);
                return new List<FineViewModel>();
            }
        }

        public async Task<FineStatisticsViewModel> GetFineStatisticsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{GetApiBaseUrl()}/api/Fines/statistics");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<FineStatisticsViewModel>(content, _jsonOptions) ?? new FineStatisticsViewModel();
                }

                _logger.LogWarning("Failed to get fine statistics. Status: {StatusCode}", response.StatusCode);
                return new FineStatisticsViewModel();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting fine statistics");
                return new FineStatisticsViewModel();
            }
        }
    }

    // Response models
    public class FineListResponse
    {
        public List<FineViewModel> Fines { get; set; } = new List<FineViewModel>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

}
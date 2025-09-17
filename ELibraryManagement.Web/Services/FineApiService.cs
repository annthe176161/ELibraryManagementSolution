using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ELibraryManagement.Web.Services
{
    public interface IFineApiService
    {
        Task<(List<FineViewModel> fines, int totalCount, int totalPages)> GetAllFinesAsync(int page = 1, int pageSize = 20, string? status = null, string? search = null);
        Task<FineDetailViewModel?> GetFineDetailsAsync(int id);
        Task<bool> CreateFineAsync(CreateFineRequest request);
        Task<bool> UpdateFineAsync(int id, UpdateFineRequest request);
        Task<bool> MarkFineAsPaidAsync(int id, string? notes = null);
        Task<bool> WaiveFineAsync(int id, string reason, string? notes = null);
        Task<List<FineViewModel>> GetUserFinesAsync(string userId);
        Task<FineStatisticsViewModel> GetFineStatisticsAsync();
    }

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
                var response = await _httpClient.GetAsync($"{GetApiBaseUrl()}/api/Fine?{queryString}");

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
                var response = await _httpClient.GetAsync($"{GetApiBaseUrl()}/api/Fine/{id}");

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

                var response = await _httpClient.PostAsync($"{GetApiBaseUrl()}/api/Fine", content);

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

                var response = await _httpClient.PutAsync($"{GetApiBaseUrl()}/api/Fine/{id}", content);

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
                var request = new { notes };
                var json = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{GetApiBaseUrl()}/api/Fine/{id}/pay", content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Fine marked as paid successfully for ID: {FineId}", id);
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

                var response = await _httpClient.PostAsync($"{GetApiBaseUrl()}/api/Fine/{id}/waive", content);

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
                var response = await _httpClient.GetAsync($"{GetApiBaseUrl()}/api/Fine/user/{userId}");

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
                var response = await _httpClient.GetAsync($"{GetApiBaseUrl()}/api/Fine/statistics");

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

    // View models
    public class FineViewModel
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserFullName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public int? BorrowRecordId { get; set; }
        public string? BookTitle { get; set; }
        public decimal Amount { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime FineDate { get; set; }
        public DateTime? PaidDate { get; set; }
        public DateTime? DueDate { get; set; }
        public int ReminderCount { get; set; }
        public DateTime? LastReminderDate { get; set; }
        public string? EscalationReason { get; set; }
        public DateTime? EscalationDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsOverdue { get; set; }
    }

    public class FineDetailViewModel : FineViewModel
    {
        public string? BookAuthor { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<FineActionHistoryViewModel> ActionHistory { get; set; } = new List<FineActionHistoryViewModel>();
    }

    public class FineActionHistoryViewModel
    {
        public int Id { get; set; }
        public string ActionType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal? Amount { get; set; }
        public string? Notes { get; set; }
        public DateTime ActionDate { get; set; }
        public string UserFullName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
    }

    public class FineStatisticsViewModel
    {
        public int TotalFines { get; set; }
        public int PendingFines { get; set; }
        public int PaidFines { get; set; }
        public int WaivedFines { get; set; }
        public int OverdueFines { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal PendingAmount { get; set; }
    }

    // Request models
    public class CreateFineRequest
    {
        public string UserId { get; set; } = string.Empty;
        public int? BorrowRecordId { get; set; }
        public decimal Amount { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
    }

    public class UpdateFineRequest
    {
        public decimal Amount { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
        public string? Status { get; set; }
        public string? Notes { get; set; }
    }
}
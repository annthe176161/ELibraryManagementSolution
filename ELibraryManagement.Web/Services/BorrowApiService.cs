using ELibraryManagement.Web.Models;
using System.Text.Json;
using System.Text;

namespace ELibraryManagement.Web.Services
{
    public interface IBorrowApiService
    {
        Task<ExtendBorrowResponseViewModel> ExtendBorrowAsync(int borrowId, string? reason = null);
        Task<bool> IsAuthenticatedAsync();
    }

    public class BorrowApiService : IBorrowApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly JsonSerializerOptions _jsonOptions;

        public BorrowApiService(HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        private string GetApiBaseUrl()
        {
            var httpsUrl = _configuration["ApiSettings:BaseUrl"];
            var httpUrl = _configuration["ApiSettings:BaseUrlHttp"];
            return httpsUrl ?? httpUrl ?? "https://localhost:7125";
        }

        private string? GetCurrentToken()
        {
            return _httpContextAccessor.HttpContext?.Session.GetString("Token");
        }

        public async Task<bool> IsAuthenticatedAsync()
        {
            var token = GetCurrentToken();
            return !string.IsNullOrEmpty(token);
        }

        public async Task<ExtendBorrowResponseViewModel> ExtendBorrowAsync(int borrowId, string? reason = null)
        {
            try
            {
                var token = GetCurrentToken();
                if (string.IsNullOrEmpty(token))
                {
                    return new ExtendBorrowResponseViewModel
                    {
                        Success = false,
                        Message = "Vui lòng đăng nhập để thực hiện chức năng này."
                    };
                }

                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var requestData = new { Reason = reason };
                var json = JsonSerializer.Serialize(requestData, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{GetApiBaseUrl()}/api/Borrow/{borrowId}/extend", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<ExtendBorrowResponseViewModel>(responseContent, _jsonOptions);
                    return result ?? new ExtendBorrowResponseViewModel { Success = true, Message = "Gia hạn thành công!" };
                }

                return new ExtendBorrowResponseViewModel
                {
                    Success = false,
                    Message = $"Gia hạn thất bại: {responseContent}"
                };
            }
            catch (Exception ex)
            {
                return new ExtendBorrowResponseViewModel
                {
                    Success = false,
                    Message = $"Có lỗi xảy ra: {ex.Message}"
                };
            }
        }
    }
}
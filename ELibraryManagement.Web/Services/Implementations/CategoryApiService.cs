using ELibraryManagement.Web.Models.DTOs.CategoryDtos;
using ELibraryManagement.Web.Services.Interfaces;
using Newtonsoft.Json;
using System.Text;

namespace ELibraryManagement.Web.Services.Implementations
{
    public class CategoryApiService : ICategoryApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CategoryApiService> _logger;
        private readonly IAuthApiService _authApiService;

        public CategoryApiService(HttpClient httpClient, IConfiguration configuration, ILogger<CategoryApiService> logger, IAuthApiService authApiService)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _authApiService = authApiService;
        }

        private string GetApiBaseUrl()
        {
            // Ưu tiên HTTPS URL cho Visual Studio 2022, fallback về HTTP cho VS Code
            var httpsUrl = _configuration["ApiSettings:BaseUrl"];
            var httpUrl = _configuration["ApiSettings:BaseUrlHttp"];

            // Sử dụng BaseUrl đầu tiên (ưu tiên HTTPS), nếu không có thì dùng HTTP
            return httpsUrl ?? httpUrl ?? "http://localhost:5293";
        }

        private void SetAuthorizationHeader()
        {
            var token = _authApiService.GetCurrentToken();
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<CategoriesListResponseDto> GetAllCategoriesAsync(bool includeInactive = false)
        {
            try
            {
                SetAuthorizationHeader();

                var baseUrl = GetApiBaseUrl();
                var url = $"{baseUrl}/api/Category?includeInactive={includeInactive}";

                var response = await _httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<CategoriesListResponseDto>(content);
                    return result ?? new CategoriesListResponseDto { Success = false, Message = "Không thể deserialize dữ liệu" };
                }

                _logger.LogError("API call failed: {StatusCode} - {Content}", response.StatusCode, content);
                return new CategoriesListResponseDto
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi lấy danh sách danh mục"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling GetAllCategories API");
                return new CategoriesListResponseDto
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi kết nối với máy chủ"
                };
            }
        }

        public async Task<CategoryResponseDto> GetCategoryByIdAsync(int id)
        {
            try
            {
                var baseUrl = GetApiBaseUrl();
                var response = await _httpClient.GetAsync($"{baseUrl}/api/Category/{id}");
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<CategoryResponseDto>(content);
                    return result ?? new CategoryResponseDto { Success = false, Message = "Không thể deserialize dữ liệu" };
                }

                _logger.LogError("API call failed: {StatusCode} - {Content}", response.StatusCode, content);
                return new CategoryResponseDto
                {
                    Success = false,
                    Message = "Không tìm thấy danh mục"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling GetCategoryById API for id {CategoryId}", id);
                return new CategoryResponseDto
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi kết nối với máy chủ"
                };
            }
        }

        public async Task<CategoryResponseDto> CreateCategoryAsync(CreateCategoryDto createDto)
        {
            try
            {
                SetAuthorizationHeader();

                var json = JsonConvert.SerializeObject(createDto);
                var httpContent = new StringContent(json, Encoding.UTF8, System.Net.Mime.MediaTypeNames.Application.Json);

                var baseUrl = GetApiBaseUrl();
                var response = await _httpClient.PostAsync($"{baseUrl}/api/Category", httpContent);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<CategoryResponseDto>(content);
                    return result ?? new CategoryResponseDto { Success = false, Message = "Không thể deserialize dữ liệu" };
                }

                _logger.LogError("API call failed: {StatusCode} - {Content}", response.StatusCode, content);

                // Try to parse error message
                try
                {
                    var errorResult = JsonConvert.DeserializeObject<CategoryResponseDto>(content);
                    if (errorResult != null && !string.IsNullOrEmpty(errorResult.Message))
                    {
                        return errorResult;
                    }
                }
                catch
                {
                    // Ignore deserialization errors
                }

                return new CategoryResponseDto
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi tạo danh mục"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling CreateCategory API");
                return new CategoryResponseDto
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi kết nối với máy chủ"
                };
            }
        }

        public async Task<CategoryResponseDto> UpdateCategoryAsync(int id, UpdateCategoryDto updateDto)
        {
            try
            {
                SetAuthorizationHeader();

                var json = JsonConvert.SerializeObject(updateDto);
                var httpContent = new StringContent(json, Encoding.UTF8, System.Net.Mime.MediaTypeNames.Application.Json);

                var baseUrl = GetApiBaseUrl();
                var response = await _httpClient.PutAsync($"{baseUrl}/api/Category/{id}", httpContent);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<CategoryResponseDto>(content);
                    return result ?? new CategoryResponseDto { Success = false, Message = "Không thể deserialize dữ liệu" };
                }

                _logger.LogError("API call failed: {StatusCode} - {Content}", response.StatusCode, content);

                // Try to parse error message
                try
                {
                    var errorResult = JsonConvert.DeserializeObject<CategoryResponseDto>(content);
                    if (errorResult != null && !string.IsNullOrEmpty(errorResult.Message))
                    {
                        return errorResult;
                    }
                }
                catch
                {
                    // Ignore deserialization errors
                }

                return new CategoryResponseDto
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi cập nhật danh mục"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling UpdateCategory API for id {CategoryId}", id);
                return new CategoryResponseDto
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi kết nối với máy chủ"
                };
            }
        }

        public async Task<CategoryResponseDto> DeleteCategoryAsync(int id)
        {
            try
            {
                SetAuthorizationHeader();

                var baseUrl = GetApiBaseUrl();
                var response = await _httpClient.DeleteAsync($"{baseUrl}/api/Category/{id}");
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<CategoryResponseDto>(content);
                    return result ?? new CategoryResponseDto { Success = false, Message = "Không thể deserialize dữ liệu" };
                }

                _logger.LogError("API call failed: {StatusCode} - {Content}", response.StatusCode, content);

                // Try to parse error message
                try
                {
                    var errorResult = JsonConvert.DeserializeObject<CategoryResponseDto>(content);
                    if (errorResult != null && !string.IsNullOrEmpty(errorResult.Message))
                    {
                        return errorResult;
                    }
                }
                catch
                {
                    // Ignore deserialization errors
                }

                return new CategoryResponseDto
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi xóa danh mục"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling DeleteCategory API for id {CategoryId}", id);
                return new CategoryResponseDto
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi kết nối với máy chủ"
                };
            }
        }

        public async Task<CategoryResponseDto> ToggleCategoryStatusAsync(int id)
        {
            try
            {
                SetAuthorizationHeader();

                var baseUrl = GetApiBaseUrl();
                var response = await _httpClient.PatchAsync($"{baseUrl}/api/Category/{id}/toggle-status", null);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<CategoryResponseDto>(content);
                    return result ?? new CategoryResponseDto { Success = false, Message = "Không thể deserialize dữ liệu" };
                }

                _logger.LogError("API call failed: {StatusCode} - {Content}", response.StatusCode, content);

                // Try to parse error message
                try
                {
                    var errorResult = JsonConvert.DeserializeObject<CategoryResponseDto>(content);
                    if (errorResult != null && !string.IsNullOrEmpty(errorResult.Message))
                    {
                        return errorResult;
                    }
                }
                catch
                {
                    // Ignore deserialization errors
                }

                return new CategoryResponseDto
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi thay đổi trạng thái danh mục"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling ToggleCategoryStatus API for id {CategoryId}", id);
                return new CategoryResponseDto
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi kết nối với máy chủ"
                };
            }
        }

        public async Task<bool> CheckCategoryNameExistsAsync(string name, int? excludeId = null)
        {
            try
            {
                SetAuthorizationHeader();

                var baseUrl = GetApiBaseUrl();
                var url = $"{baseUrl}/api/Category/check-name?name={Uri.EscapeDataString(name)}";
                if (excludeId.HasValue)
                {
                    url += $"&excludeId={excludeId.Value}";
                }

                var response = await _httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<dynamic>(content);
                    return result?.exists ?? false;
                }

                _logger.LogError("API call failed: {StatusCode} - {Content}", response.StatusCode, content);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling CheckCategoryNameExists API for name {CategoryName}", name);
                return false;
            }
        }
    }
}
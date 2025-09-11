using ELibraryManagement.Web.Models;
using System.Text.Json;
using System.Text;

namespace ELibraryManagement.Web.Services
{
    public interface IAuthApiService
    {
        Task<AuthResponseViewModel> RegisterAsync(RegisterViewModel model);
        Task<AuthResponseViewModel> LoginAsync(LoginViewModel model);
        Task<UserViewModel?> GetCurrentUserAsync();
        void Logout();
        bool IsAuthenticated();
        string? GetCurrentUserToken();
        string GetCurrentUserName();
    }

    public class AuthApiService : IAuthApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly JsonSerializerOptions _jsonOptions;

        public AuthApiService(HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<AuthResponseViewModel> RegisterAsync(RegisterViewModel model)
        {
            try
            {
                var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5293";
                var requestData = new
                {
                    email = model.Email,
                    password = model.Password,
                    confirmPassword = model.ConfirmPassword,
                    userName = model.UserName,
                    firstName = model.FirstName,
                    lastName = model.LastName,
                    address = model.Address,
                    phoneNumber = model.PhoneNumber,
                    dateOfBirth = model.DateOfBirth
                };

                var json = JsonSerializer.Serialize(requestData, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{apiBaseUrl}/api/Auth/register", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonSerializer.Deserialize<JsonElement>(responseContent, _jsonOptions);
                    return new AuthResponseViewModel
                    {
                        Success = true,
                        Message = "Đăng ký thành công! Bạn có thể đăng nhập ngay bây giờ.",
                        Token = GetJsonProperty<string>(apiResponse, "token"),
                        User = ParseUser(apiResponse)
                    };
                }
                else
                {
                    var errorResponse = JsonSerializer.Deserialize<JsonElement>(responseContent, _jsonOptions);
                    return new AuthResponseViewModel
                    {
                        Success = false,
                        Message = GetJsonProperty<string>(errorResponse, "message") ?? "Đăng ký thất bại"
                    };
                }
            }
            catch (Exception ex)
            {
                return new AuthResponseViewModel
                {
                    Success = false,
                    Message = $"Lỗi kết nối: {ex.Message}"
                };
            }
        }

        public async Task<AuthResponseViewModel> LoginAsync(LoginViewModel model)
        {
            try
            {
                var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5293";
                var requestData = new
                {
                    userNameOrEmail = model.UserNameOrEmail,
                    password = model.Password
                };

                var json = JsonSerializer.Serialize(requestData, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{apiBaseUrl}/api/Auth/login", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonSerializer.Deserialize<JsonElement>(responseContent, _jsonOptions);
                    var token = GetJsonProperty<string>(apiResponse, "token");
                    var user = ParseUser(apiResponse);

                    if (!string.IsNullOrEmpty(token))
                    {
                        // Store token and user info in session
                        _httpContextAccessor.HttpContext?.Session.SetString("AuthToken", token);
                        if (user != null)
                        {
                            _httpContextAccessor.HttpContext?.Session.SetString("UserName", user.UserName ?? "");
                            _httpContextAccessor.HttpContext?.Session.SetString("Email", user.Email ?? "");
                            _httpContextAccessor.HttpContext?.Session.SetString("FullName",
                                !string.IsNullOrEmpty(user.FirstName) || !string.IsNullOrEmpty(user.LastName)
                                    ? $"{user.FirstName} {user.LastName}".Trim()
                                    : user.UserName ?? user.Email ?? "");
                        }
                    }

                    return new AuthResponseViewModel
                    {
                        Success = true,
                        Message = "Đăng nhập thành công!",
                        Token = token,
                        User = user
                    };
                }
                else
                {
                    var errorResponse = JsonSerializer.Deserialize<JsonElement>(responseContent, _jsonOptions);
                    return new AuthResponseViewModel
                    {
                        Success = false,
                        Message = GetJsonProperty<string>(errorResponse, "message") ?? "Đăng nhập thất bại"
                    };
                }
            }
            catch (Exception ex)
            {
                return new AuthResponseViewModel
                {
                    Success = false,
                    Message = $"Lỗi kết nối: {ex.Message}"
                };
            }
        }

        public async Task<UserViewModel?> GetCurrentUserAsync()
        {
            try
            {
                var token = GetCurrentUserToken();
                if (string.IsNullOrEmpty(token))
                    return null;

                var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5293";
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

                var response = await _httpClient.GetAsync($"{apiBaseUrl}/api/Auth/me");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);
                    return ParseUser(apiResponse);
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        public void Logout()
        {
            _httpContextAccessor.HttpContext?.Session.Remove("AuthToken");
            _httpContextAccessor.HttpContext?.Session.Remove("UserName");
            _httpContextAccessor.HttpContext?.Session.Remove("Email");
            _httpContextAccessor.HttpContext?.Session.Remove("FullName");
            _httpClient.DefaultRequestHeaders.Clear();
        }

        public bool IsAuthenticated()
        {
            return !string.IsNullOrEmpty(GetCurrentUserToken());
        }

        public string? GetCurrentUserToken()
        {
            return _httpContextAccessor.HttpContext?.Session.GetString("AuthToken");
        }

        private T GetJsonProperty<T>(JsonElement element, string propertyName)
        {
            try
            {
                if (element.TryGetProperty(propertyName, out var property))
                {
                    if (typeof(T) == typeof(string))
                    {
                        return (T)(object)(property.GetString() ?? string.Empty);
                    }
                    else if (typeof(T) == typeof(int))
                    {
                        return (T)(object)property.GetInt32();
                    }
                    else if (typeof(T) == typeof(DateTime))
                    {
                        return (T)(object)property.GetDateTime();
                    }
                    else if (typeof(T) == typeof(DateTime?))
                    {
                        return property.ValueKind == JsonValueKind.Null ? default(T)! : (T)(object)property.GetDateTime();
                    }
                }
                return default(T)!;
            }
            catch
            {
                return default(T)!;
            }
        }

        private UserViewModel? ParseUser(JsonElement response)
        {
            try
            {
                if (response.TryGetProperty("user", out var userElement) || response.TryGetProperty("data", out userElement))
                {
                    return new UserViewModel
                    {
                        Id = GetJsonProperty<string>(userElement, "id"),
                        UserName = GetJsonProperty<string>(userElement, "userName"),
                        Email = GetJsonProperty<string>(userElement, "email"),
                        FirstName = GetJsonProperty<string>(userElement, "firstName"),
                        LastName = GetJsonProperty<string>(userElement, "lastName"),
                        Address = GetJsonProperty<string>(userElement, "address"),
                        PhoneNumber = GetJsonProperty<string>(userElement, "phoneNumber"),
                        DateOfBirth = GetJsonProperty<DateTime?>(userElement, "dateOfBirth"),
                        AvatarUrl = GetJsonProperty<string>(userElement, "avatarUrl"),
                        CreatedAt = GetJsonProperty<DateTime>(userElement, "createdAt")
                    };
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public string GetCurrentUserName()
        {
            try
            {
                var context = _httpContextAccessor.HttpContext;
                if (context?.Session != null)
                {
                    var fullName = context.Session.GetString("FullName");
                    var userName = context.Session.GetString("UserName");
                    var email = context.Session.GetString("Email");
                    return fullName ?? userName ?? email ?? "User";
                }
                return "User";
            }
            catch
            {
                return "User";
            }
        }
    }
}

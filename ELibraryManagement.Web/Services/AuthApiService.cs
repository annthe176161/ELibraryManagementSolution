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
        string? GetCurrentToken();
        Task<List<string>> GetCurrentUserRolesAsync();
        Task<bool> IsInRoleAsync(string roleName);
        Task<AuthResponseViewModel> UpdateProfileAsync(EditProfileViewModel model);
        Task<AuthResponseViewModel> ChangePasswordAsync(ChangePasswordViewModel model);
        Task<AuthResponseViewModel> ForgotPasswordAsync(ForgotPasswordViewModel model);
        Task<AuthResponseViewModel> ResetPasswordAsync(ResetPasswordViewModel model);
        Task<AuthResponseViewModel> UploadAvatarAsync(IFormFile file);
        void StoreToken(string token);
        void StoreUserSession(string token, UserViewModel user);
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

        private string GetApiBaseUrl()
        {
            // Ưu tiên HTTPS URL cho Visual Studio 2022, fallback về HTTP cho VS Code
            var httpsUrl = _configuration["ApiSettings:BaseUrl"];
            var httpUrl = _configuration["ApiSettings:BaseUrlHttp"];

            // Sử dụng BaseUrl đầu tiên (ưu tiên HTTPS), nếu không có thì dùng HTTP
            return httpsUrl ?? httpUrl ?? "https://localhost:7125";
        }

        public async Task<AuthResponseViewModel> RegisterAsync(RegisterViewModel model)
        {
            try
            {
                var apiBaseUrl = GetApiBaseUrl();
                var requestData = new
                {
                    email = model.Email,
                    password = model.Password,
                    confirmPassword = model.ConfirmPassword,
                    userName = model.UserName,
                    firstName = model.FirstName,
                    lastName = model.LastName,
                    studentId = model.StudentId,
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
                var apiBaseUrl = GetApiBaseUrl();
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
                {
                    // If no token, ensure session is cleared
                    Logout();
                    return null;
                }

                var apiBaseUrl = GetApiBaseUrl();

                // Clear existing headers
                _httpClient.DefaultRequestHeaders.Clear();

                // Set Authorization header properly
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync($"{apiBaseUrl}/api/Auth/me");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);
                    var user = ParseUser(apiResponse);

                    if (user != null)
                    {
                        // Update session with fresh user info
                        _httpContextAccessor.HttpContext?.Session.SetString("UserName", user.UserName ?? "");
                        _httpContextAccessor.HttpContext?.Session.SetString("Email", user.Email ?? "");
                        _httpContextAccessor.HttpContext?.Session.SetString("FullName",
                            !string.IsNullOrEmpty(user.FirstName) || !string.IsNullOrEmpty(user.LastName)
                                ? $"{user.FirstName} {user.LastName}".Trim()
                                : user.UserName ?? user.Email ?? "");
                    }

                    return user;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    // Token is invalid or expired, clear session
                    Logout();
                    return null;
                }

                return null;
            }
            catch (Exception ex)
            {
                // Log the error if needed
                System.Diagnostics.Debug.WriteLine($"Error in GetCurrentUserAsync: {ex.Message}");
                // Clear session on error to avoid inconsistent state
                Logout();
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
            try
            {
                var token = GetCurrentUserToken();
                var context = _httpContextAccessor.HttpContext;
                var userName = context?.Session?.GetString("UserName");

                var isAuth = !string.IsNullOrEmpty(token) &&
                       context?.Session != null &&
                       !string.IsNullOrEmpty(userName);

                // Debug logging
                System.Diagnostics.Debug.WriteLine($"IsAuthenticated check - Token: {!string.IsNullOrEmpty(token)}, Session: {context?.Session != null}, UserName: {!string.IsNullOrEmpty(userName)}, Result: {isAuth}");

                return isAuth;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"IsAuthenticated error: {ex.Message}");
                return false;
            }
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
                        StudentId = GetJsonProperty<string>(userElement, "studentId"),
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

                    // Clean the name by removing any leading/trailing dashes and extra spaces
                    var name = fullName ?? userName ?? email ?? "User";
                    name = name.Trim().TrimStart('-').TrimEnd('-').Trim();

                    return string.IsNullOrEmpty(name) ? "User" : name;
                }
                return "User";
            }
            catch
            {
                return "User";
            }
        }

        public string? GetCurrentToken()
        {
            return GetCurrentUserToken();
        }

        public async Task<List<string>> GetCurrentUserRolesAsync()
        {
            try
            {
                var token = GetCurrentUserToken();
                if (string.IsNullOrEmpty(token))
                    return new List<string>();

                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync($"{GetApiBaseUrl()}/api/Auth/roles");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(content, _jsonOptions);
                    return result?["roles"] ?? new List<string>();
                }
                return new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }

        public async Task<bool> IsInRoleAsync(string roleName)
        {
            var roles = await GetCurrentUserRolesAsync();
            return roles.Contains(roleName, StringComparer.OrdinalIgnoreCase);
        }

        public async Task<AuthResponseViewModel> UpdateProfileAsync(EditProfileViewModel model)
        {
            try
            {
                var token = GetCurrentToken();
                if (string.IsNullOrEmpty(token))
                {
                    return new AuthResponseViewModel
                    {
                        Success = false,
                        Message = "Bạn cần đăng nhập để cập nhật thông tin."
                    };
                }

                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var json = JsonSerializer.Serialize(model, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"{GetApiBaseUrl()}/api/Auth/update-profile", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<AuthResponseViewModel>(responseContent, _jsonOptions);
                    return result ?? new AuthResponseViewModel { Success = true, Message = "Cập nhật thông tin thành công!" };
                }

                return new AuthResponseViewModel
                {
                    Success = false,
                    Message = $"Cập nhật thông tin thất bại: {responseContent}"
                };
            }
            catch (Exception ex)
            {
                return new AuthResponseViewModel
                {
                    Success = false,
                    Message = $"Có lỗi xảy ra: {ex.Message}"
                };
            }
        }

        public async Task<AuthResponseViewModel> ChangePasswordAsync(ChangePasswordViewModel model)
        {
            try
            {
                var token = GetCurrentToken();
                if (string.IsNullOrEmpty(token))
                {
                    return new AuthResponseViewModel
                    {
                        Success = false,
                        Message = "Bạn cần đăng nhập để đổi mật khẩu."
                    };
                }

                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var json = JsonSerializer.Serialize(model, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"{GetApiBaseUrl()}/api/Auth/change-password", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<AuthResponseViewModel>(responseContent, _jsonOptions);
                    return result ?? new AuthResponseViewModel { Success = true, Message = "Đổi mật khẩu thành công!" };
                }

                return new AuthResponseViewModel
                {
                    Success = false,
                    Message = $"Đổi mật khẩu thất bại: {responseContent}"
                };
            }
            catch (Exception ex)
            {
                return new AuthResponseViewModel
                {
                    Success = false,
                    Message = $"Có lỗi xảy ra: {ex.Message}"
                };
            }
        }

        public async Task<AuthResponseViewModel> ForgotPasswordAsync(ForgotPasswordViewModel model)
        {
            try
            {
                var requestData = new
                {
                    Email = model.Email
                };

                var json = JsonSerializer.Serialize(requestData, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{GetApiBaseUrl()}/api/Auth/forgot-password", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<AuthResponseViewModel>(responseContent, _jsonOptions);
                    return result ?? new AuthResponseViewModel { Success = true, Message = "Đã gửi link reset mật khẩu!" };
                }

                return new AuthResponseViewModel
                {
                    Success = false,
                    Message = $"Gửi link reset mật khẩu thất bại: {responseContent}"
                };
            }
            catch (Exception ex)
            {
                return new AuthResponseViewModel
                {
                    Success = false,
                    Message = $"Có lỗi xảy ra: {ex.Message}"
                };
            }
        }

        public async Task<AuthResponseViewModel> ResetPasswordAsync(ResetPasswordViewModel model)
        {
            try
            {
                var requestData = new
                {
                    Email = model.Email,
                    Token = model.Token,
                    NewPassword = model.NewPassword,
                    ConfirmNewPassword = model.ConfirmPassword
                };

                var json = JsonSerializer.Serialize(requestData, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{GetApiBaseUrl()}/api/Auth/reset-password", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<AuthResponseViewModel>(responseContent, _jsonOptions);
                    return result ?? new AuthResponseViewModel { Success = true, Message = "Reset mật khẩu thành công!" };
                }

                return new AuthResponseViewModel
                {
                    Success = false,
                    Message = $"Reset mật khẩu thất bại: {responseContent}"
                };
            }
            catch (Exception ex)
            {
                return new AuthResponseViewModel
                {
                    Success = false,
                    Message = $"Có lỗi xảy ra: {ex.Message}"
                };
            }
        }

        public async Task<AuthResponseViewModel> UploadAvatarAsync(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return new AuthResponseViewModel
                    {
                        Success = false,
                        Message = "Vui lòng chọn file ảnh!"
                    };
                }

                var token = GetCurrentToken();
                if (string.IsNullOrEmpty(token))
                {
                    return new AuthResponseViewModel
                    {
                        Success = false,
                        Message = "Bạn cần đăng nhập để thực hiện chức năng này!"
                    };
                }

                using var formData = new MultipartFormDataContent();
                using var fileStream = file.OpenReadStream();
                using var streamContent = new StreamContent(fileStream);
                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
                formData.Add(streamContent, "file", file.FileName);

                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.PostAsync($"{GetApiBaseUrl()}/api/User/upload-avatar", formData);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<AuthResponseViewModel>(responseContent, _jsonOptions);
                    return result ?? new AuthResponseViewModel { Success = true, Message = "Upload avatar thành công!" };
                }

                return new AuthResponseViewModel
                {
                    Success = false,
                    Message = $"Upload avatar thất bại: {responseContent}"
                };
            }
            catch (Exception ex)
            {
                return new AuthResponseViewModel
                {
                    Success = false,
                    Message = $"Có lỗi xảy ra: {ex.Message}"
                };
            }
        }

        public void StoreToken(string token)
        {
            _httpContextAccessor.HttpContext?.Session.SetString("AuthToken", token);
        }

        public void StoreUserSession(string token, UserViewModel user)
        {
            _httpContextAccessor.HttpContext?.Session.SetString("AuthToken", token);
            _httpContextAccessor.HttpContext?.Session.SetString("UserName", user.UserName ?? "");
            _httpContextAccessor.HttpContext?.Session.SetString("Email", user.Email ?? "");
            _httpContextAccessor.HttpContext?.Session.SetString("FullName",
                $"{user.FirstName} {user.LastName}".Trim());
        }
    }
}

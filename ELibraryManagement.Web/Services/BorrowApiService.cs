using ELibraryManagement.Web.Models;
using System.Text.Json;
using System.Text;

namespace ELibraryManagement.Web.Services
{
    // DTO to match API response
    public class BorrowRecordDto
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public string BookAuthor { get; set; } = string.Empty;
        public string BookCoverUrl { get; set; } = string.Empty;
        public string BookIsbn { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string UserPhoneNumber { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public DateTime BorrowDate { get; set; }
        public DateTime? ConfirmedDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public bool IsOverdue => ReturnDate == null && DateTime.UtcNow > DueDate;
        public int OverdueDays => IsOverdue ? (DateTime.UtcNow - DueDate).Days : 0;
        public decimal? FineAmount { get; set; }
        public string? FineStatus { get; set; }
        public string? FineReason { get; set; }
    }

    public class BorrowApiService : IBorrowApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthApiService _authApiService;
        private readonly JsonSerializerOptions _jsonOptions;

        public BorrowApiService(HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor, IAuthApiService authApiService)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _authApiService = authApiService;
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

        private void SetAuthorizationHeader()
        {
            var token = _authApiService.GetCurrentToken();
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
        }

        private string? GetCurrentToken()
        {
            return _authApiService.GetCurrentToken();
        }

        public async Task<bool> IsAuthenticatedAsync()
        {
            // Kiểm tra token từ cả Session và Cookie
            var sessionToken = _httpContextAccessor.HttpContext?.Session.GetString("Token");
            var cookieToken = _httpContextAccessor.HttpContext?.Request.Cookies["AuthToken"];

            var token = !string.IsNullOrEmpty(sessionToken) ? sessionToken : cookieToken;

            return await Task.FromResult(!string.IsNullOrEmpty(token));
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

        public async Task<BorrowResult> BorrowBookAsync(int bookId)
        {
            try
            {
                var token = _httpContextAccessor.HttpContext?.Request.Cookies["AuthToken"];
                if (string.IsNullOrEmpty(token))
                {
                    return new BorrowResult { Success = false, Message = "Vui lòng đăng nhập để mượn sách." };
                }

                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var apiUrl = $"{_configuration["ApiSettings:BaseUrl"]}/api/Borrow/borrow";
                var requestContent = new StringContent(
                    JsonSerializer.Serialize(new { BookId = bookId }),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync(apiUrl, requestContent);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var jsonDocument = JsonDocument.Parse(responseContent);
                    var root = jsonDocument.RootElement;

                    return new BorrowResult
                    {
                        Success = true,
                        Message = "Mượn sách thành công!",
                        BorrowRecordId = root.TryGetProperty("borrowRecordId", out var borrowIdProp) ?
                            borrowIdProp.GetInt32() : null
                    };
                }
                else
                {
                    var errorMessage = "Không thể mượn sách";
                    if (!string.IsNullOrEmpty(responseContent))
                    {
                        try
                        {
                            var errorJson = JsonDocument.Parse(responseContent);
                            if (errorJson.RootElement.TryGetProperty("message", out var messageProp))
                            {
                                errorMessage = messageProp.GetString() ?? errorMessage;
                            }
                        }
                        catch { }
                    }

                    return new BorrowResult { Success = false, Message = errorMessage };
                }
            }
            catch (Exception ex)
            {
                return new BorrowResult { Success = false, Message = $"Có lỗi xảy ra: {ex.Message}" };
            }
        }

        public async Task<BorrowDetailViewModel?> GetBorrowDetailAsync(int borrowId)
        {
            try
            {
                var token = GetCurrentToken();
                if (string.IsNullOrEmpty(token))
                {
                    return null;
                }

                SetAuthorizationHeader();

                var response = await _httpClient.GetAsync($"{GetApiBaseUrl()}/api/Borrow/admin/{borrowId}");
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var borrowRecordDto = JsonSerializer.Deserialize<BorrowRecordDto>(responseContent, _jsonOptions);

                    if (borrowRecordDto != null)
                    {
                        // Map BorrowRecordDto to BorrowDetailViewModel
                        return new BorrowDetailViewModel
                        {
                            Id = borrowRecordDto.Id,
                            UserId = borrowRecordDto.UserId,
                            UserName = borrowRecordDto.UserName,
                            UserEmail = borrowRecordDto.UserEmail,
                            StudentId = borrowRecordDto.StudentId,
                            UserPhoneNumber = borrowRecordDto.UserPhoneNumber,
                            BookId = borrowRecordDto.BookId,
                            BookTitle = borrowRecordDto.BookTitle,
                            BookAuthor = borrowRecordDto.BookAuthor,
                            BookCoverUrl = borrowRecordDto.BookCoverUrl,
                            BookIsbn = borrowRecordDto.BookIsbn,
                            BorrowDate = borrowRecordDto.BorrowDate,
                            ConfirmedDate = borrowRecordDto.ConfirmedDate,
                            DueDate = borrowRecordDto.DueDate,
                            ReturnDate = borrowRecordDto.ReturnDate,
                            Status = borrowRecordDto.Status,
                            Notes = borrowRecordDto.Notes,
                            FineAmount = borrowRecordDto.FineAmount
                        };
                    }
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> UpdateBorrowStatusAsync(int borrowId, string status, string? notes = null)
        {
            try
            {
                var token = GetCurrentToken();
                if (string.IsNullOrEmpty(token))
                {
                    Console.WriteLine("No token available for UpdateBorrowStatusAsync");
                    return false;
                }

                SetAuthorizationHeader();

                var requestData = new { Status = status, Notes = notes };
                var json = JsonSerializer.Serialize(requestData, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                Console.WriteLine($"Sending PUT request to: {GetApiBaseUrl()}/api/Borrow/admin/{borrowId}/status");
                Console.WriteLine($"Request payload: {json}");

                var response = await _httpClient.PutAsync($"{GetApiBaseUrl()}/api/Borrow/admin/{borrowId}/status", content);

                Console.WriteLine($"Response status: {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error response: {errorContent}");
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in UpdateBorrowStatusAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ApproveBorrowRequestAsync(int borrowId)
        {
            try
            {
                var token = GetCurrentToken();
                if (string.IsNullOrEmpty(token))
                {
                    return false;
                }

                SetAuthorizationHeader();

                var response = await _httpClient.PostAsync($"{GetApiBaseUrl()}/api/borrow/approve/{borrowId}", null);
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                // Try to read error message from API and log it for the UI
                var errorContent = await response.Content.ReadAsStringAsync();
                try
                {
                    var json = JsonSerializer.Deserialize<JsonElement>(errorContent, _jsonOptions);
                    if (json.ValueKind == JsonValueKind.Object && json.TryGetProperty("message", out var msg))
                    {
                        // Store last error in session so AdminController can retrieve it if needed
                        _httpContextAccessor.HttpContext?.Session.SetString("LastApproveError", msg.GetString() ?? "");
                    }
                }
                catch { }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<ReturnBookResponseViewModel?> ConfirmReturnAsync(int borrowId)
        {
            try
            {
                var token = GetCurrentToken();
                if (string.IsNullOrEmpty(token))
                {
                    return null;
                }

                SetAuthorizationHeader();

                var response = await _httpClient.PostAsync($"{GetApiBaseUrl()}/api/Borrow/return/{borrowId}", null);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<ReturnBookResponseViewModel>(responseContent, _jsonOptions);
                    return result;
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<List<BorrowRecordViewModel>?> GetAllBorrowRecordsAsync()
        {
            try
            {
                var token = GetCurrentToken();
                if (string.IsNullOrEmpty(token))
                {
                    return null;
                }

                SetAuthorizationHeader();

                var response = await _httpClient.GetAsync($"{GetApiBaseUrl()}/api/Borrow/admin/all");
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var borrowRecords = JsonSerializer.Deserialize<List<BorrowRecordViewModel>>(responseContent, _jsonOptions);
                    return borrowRecords;
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<BorrowRecordViewModel?> GetBorrowRecordByIdAsync(int id)
        {
            try
            {
                var token = GetCurrentToken();
                if (string.IsNullOrEmpty(token))
                {
                    return null;
                }

                SetAuthorizationHeader();

                var response = await _httpClient.GetAsync($"{GetApiBaseUrl()}/api/Borrow/admin/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var borrowRecord = JsonSerializer.Deserialize<BorrowRecordViewModel>(responseContent, _jsonOptions);
                    return borrowRecord;
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<List<BorrowRecordViewModel>?> GetOverdueBorrowsAsync()
        {
            try
            {
                var token = GetCurrentToken();
                if (string.IsNullOrEmpty(token))
                {
                    return null;
                }

                SetAuthorizationHeader();

                var response = await _httpClient.GetAsync($"{GetApiBaseUrl()}/api/Borrow/overdue");
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var borrowRecords = JsonSerializer.Deserialize<List<BorrowRecordViewModel>>(responseContent, _jsonOptions);
                    return borrowRecords;
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> SendReminderAsync(int borrowId)
        {
            try
            {
                var token = GetCurrentToken();
                if (string.IsNullOrEmpty(token))
                {
                    return false;
                }

                SetAuthorizationHeader();

                var response = await _httpClient.PostAsync($"{GetApiBaseUrl()}/api/Borrow/{borrowId}/reminder", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> CancelBorrowRequestAsync(int borrowId)
        {
            try
            {
                var token = GetCurrentToken();
                if (string.IsNullOrEmpty(token))
                {
                    return false;
                }

                SetAuthorizationHeader();

                var response = await _httpClient.PostAsync($"{GetApiBaseUrl()}/api/Borrow/cancel/{borrowId}", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<List<BorrowRecordViewModel>?> GetMyBorrowsAsync()
        {
            try
            {
                var token = GetCurrentToken();
                if (string.IsNullOrEmpty(token))
                {
                    return null;
                }

                SetAuthorizationHeader();

                var response = await _httpClient.GetAsync($"{GetApiBaseUrl()}/api/Borrow/my-borrows");
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var borrowRecords = JsonSerializer.Deserialize<List<BorrowRecordViewModel>>(responseContent, _jsonOptions);
                    return borrowRecords;
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void SetAuthToken(string token)
        {
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
        }
    }
}
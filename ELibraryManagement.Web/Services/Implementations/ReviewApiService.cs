using ELibraryManagement.Web.Models;
using ELibraryManagement.Web.Services.Interfaces;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ELibraryManagement.Web.Services.Implementations
{
    public class ReviewApiService : IReviewApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly JsonSerializerOptions _jsonOptions;

        public ReviewApiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        private string GetApiBaseUrl()
        {
            return _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7125";
        }

        public async Task<ReviewResponseViewModel> CreateReviewAsync(CreateReviewViewModel createReview, string token)
        {
            try
            {
                var apiBaseUrl = GetApiBaseUrl();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var createDto = new
                {
                    BookId = createReview.BookId,
                    Rating = createReview.Rating,
                    Comment = createReview.Comment,
                    BorrowRecordId = createReview.BorrowRecordId
                };

                var json = JsonSerializer.Serialize(createDto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{apiBaseUrl}/api/Review", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonSerializer.Deserialize<dynamic>(responseContent, _jsonOptions);

                    return new ReviewResponseViewModel
                    {
                        Success = true,
                        Message = GetPropertyValue<string>(apiResponse, "message") ?? "Tạo đánh giá thành công!"
                    };
                }

                var errorResponse = JsonSerializer.Deserialize<dynamic>(responseContent, _jsonOptions);
                return new ReviewResponseViewModel
                {
                    Success = false,
                    Message = GetPropertyValue<string>(errorResponse, "message") ?? "Có lỗi xảy ra khi tạo đánh giá."
                };
            }
            catch (Exception ex)
            {
                return new ReviewResponseViewModel
                {
                    Success = false,
                    Message = $"Có lỗi xảy ra: {ex.Message}"
                };
            }
        }

        public async Task<ReviewResponseViewModel> UpdateReviewAsync(UpdateReviewViewModel updateReview, string token)
        {
            try
            {
                var apiBaseUrl = GetApiBaseUrl();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var updateDto = new
                {
                    Rating = updateReview.Rating,
                    Comment = updateReview.Comment
                };

                var json = JsonSerializer.Serialize(updateDto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"{apiBaseUrl}/api/Review/{updateReview.Id}", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return new ReviewResponseViewModel
                    {
                        Success = true,
                        Message = "Cập nhật đánh giá thành công!"
                    };
                }

                var errorResponse = JsonSerializer.Deserialize<dynamic>(responseContent, _jsonOptions);
                return new ReviewResponseViewModel
                {
                    Success = false,
                    Message = GetPropertyValue<string>(errorResponse, "message") ?? "Có lỗi xảy ra khi cập nhật đánh giá."
                };
            }
            catch (Exception ex)
            {
                return new ReviewResponseViewModel
                {
                    Success = false,
                    Message = $"Có lỗi xảy ra: {ex.Message}"
                };
            }
        }

        public async Task<ReviewResponseViewModel> DeleteReviewAsync(int reviewId, string token)
        {
            try
            {
                var apiBaseUrl = GetApiBaseUrl();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.DeleteAsync($"{apiBaseUrl}/api/Review/{reviewId}");
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return new ReviewResponseViewModel
                    {
                        Success = true,
                        Message = "Xóa đánh giá thành công!"
                    };
                }

                var errorResponse = JsonSerializer.Deserialize<dynamic>(responseContent, _jsonOptions);
                return new ReviewResponseViewModel
                {
                    Success = false,
                    Message = GetPropertyValue<string>(errorResponse, "message") ?? "Có lỗi xảy ra khi xóa đánh giá."
                };
            }
            catch (Exception ex)
            {
                return new ReviewResponseViewModel
                {
                    Success = false,
                    Message = $"Có lỗi xảy ra: {ex.Message}"
                };
            }
        }

        public async Task<ReviewViewModel?> GetReviewByIdAsync(int reviewId, string token)
        {
            try
            {
                var apiBaseUrl = GetApiBaseUrl();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await _httpClient.GetAsync($"{apiBaseUrl}/api/Review/{reviewId}");

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var reviewData = JsonSerializer.Deserialize<dynamic>(responseContent, _jsonOptions);

                    return MapToReviewViewModel(reviewData);
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<ReviewListViewModel> GetBookReviewsAsync(int bookId, int page = 1, int pageSize = 10)
        {
            try
            {
                var apiBaseUrl = GetApiBaseUrl();

                // Get reviews list
                var reviewsResponse = await _httpClient.GetAsync($"{apiBaseUrl}/api/Review/book/{bookId}?page={page}&pageSize={pageSize}");

                // Get summary data for rating statistics
                var summaryResponse = await _httpClient.GetAsync($"{apiBaseUrl}/api/Review/book/{bookId}/summary");

                if (reviewsResponse.IsSuccessStatusCode)
                {
                    var reviewsContent = await reviewsResponse.Content.ReadAsStringAsync();
                    var reviewsData = JsonSerializer.Deserialize<dynamic>(reviewsContent, _jsonOptions);

                    var reviewsList = GetPropertyValue<List<dynamic>>(reviewsData, "reviews") ?? new List<dynamic>();
                    var reviews = new List<ReviewViewModel>();
                    foreach (var reviewData in reviewsList)
                    {
                        reviews.Add(MapToReviewViewModel(reviewData));
                    }

                    var result = new ReviewListViewModel
                    {
                        Reviews = reviews,
                        TotalCount = GetPropertyValue<int>(reviewsData, "totalCount"),
                        Page = GetPropertyValue<int>(reviewsData, "page"),
                        PageSize = GetPropertyValue<int>(reviewsData, "pageSize"),
                        TotalPages = GetPropertyValue<int>(reviewsData, "totalPages"),
                        BookId = bookId
                    };

                    // Add summary data if available
                    if (summaryResponse.IsSuccessStatusCode)
                    {
                        var summaryContent = await summaryResponse.Content.ReadAsStringAsync();
                        var summaryData = JsonSerializer.Deserialize<dynamic>(summaryContent, _jsonOptions);

                        result.AverageRating = GetPropertyValue<double>(summaryData, "averageRating");

                        // Build rating distribution dictionary
                        result.RatingDistribution = new Dictionary<int, int>
                        {
                            { 5, GetPropertyValue<int>(summaryData, "fiveStarCount") },
                            { 4, GetPropertyValue<int>(summaryData, "fourStarCount") },
                            { 3, GetPropertyValue<int>(summaryData, "threeStarCount") },
                            { 2, GetPropertyValue<int>(summaryData, "twoStarCount") },
                            { 1, GetPropertyValue<int>(summaryData, "oneStarCount") }
                        };
                    }

                    return result;
                }

                return new ReviewListViewModel { BookId = bookId };
            }
            catch
            {
                return new ReviewListViewModel { BookId = bookId };
            }
        }

        public async Task<BookReviewSummaryViewModel> GetBookReviewSummaryAsync(int bookId)
        {
            try
            {
                var apiBaseUrl = GetApiBaseUrl();
                var response = await _httpClient.GetAsync($"{apiBaseUrl}/api/Review/book/{bookId}/summary");

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var data = JsonSerializer.Deserialize<dynamic>(responseContent, _jsonOptions);

                    var recentReviewsData = GetPropertyValue<List<dynamic>>(data, "recentReviews") ?? new List<dynamic>();
                    var recentReviews = new List<ReviewViewModel>();
                    foreach (var reviewData in recentReviewsData)
                    {
                        recentReviews.Add(MapToReviewViewModel(reviewData));
                    }

                    return new BookReviewSummaryViewModel
                    {
                        BookId = GetPropertyValue<int>(data, "bookId"),
                        TotalReviews = GetPropertyValue<int>(data, "totalReviews"),
                        AverageRating = GetPropertyValue<double>(data, "averageRating"),
                        FiveStarCount = GetPropertyValue<int>(data, "fiveStarCount"),
                        FourStarCount = GetPropertyValue<int>(data, "fourStarCount"),
                        ThreeStarCount = GetPropertyValue<int>(data, "threeStarCount"),
                        TwoStarCount = GetPropertyValue<int>(data, "twoStarCount"),
                        OneStarCount = GetPropertyValue<int>(data, "oneStarCount"),
                        RecentReviews = recentReviews
                    };
                }

                return new BookReviewSummaryViewModel { BookId = bookId };
            }
            catch
            {
                return new BookReviewSummaryViewModel { BookId = bookId };
            }
        }

        public async Task<List<ReviewViewModel>> GetMyReviewsAsync(string token)
        {
            try
            {
                var apiBaseUrl = GetApiBaseUrl();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync($"{apiBaseUrl}/api/Review/my-reviews");

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var reviewsData = JsonSerializer.Deserialize<List<dynamic>>(responseContent, _jsonOptions);

                    return reviewsData?.Select(MapToReviewViewModel).ToList() ?? new List<ReviewViewModel>();
                }

                return new List<ReviewViewModel>();
            }
            catch
            {
                return new List<ReviewViewModel>();
            }
        }

        public async Task<CanReviewViewModel> CanReviewBookAsync(int bookId, string token)
        {
            try
            {
                var apiBaseUrl = GetApiBaseUrl();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync($"{apiBaseUrl}/api/Review/can-review/{bookId}");

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var data = JsonSerializer.Deserialize<dynamic>(responseContent, _jsonOptions);

                    var canReview = GetPropertyValue<bool>(data, "canReview");
                    var hasExistingReview = GetPropertyValue<bool>(data, "hasExistingReview");
                    var existingReviewData = GetPropertyValue<dynamic>(data, "existingReview");

                    var result = new CanReviewViewModel
                    {
                        CanReview = canReview,
                        HasExistingReview = hasExistingReview
                    };

                    if (existingReviewData != null)
                    {
                        result.ExistingReview = MapToReviewViewModel(existingReviewData);
                    }

                    if (!canReview)
                    {
                        result.Message = "Bạn cần mượn sách này trước khi có thể viết đánh giá.";
                    }
                    else if (hasExistingReview)
                    {
                        result.Message = "Bạn đã đánh giá sách này rồi. Bạn có thể chỉnh sửa đánh giá hiện tại.";
                    }

                    return result;
                }

                return new CanReviewViewModel
                {
                    CanReview = false,
                    Message = "Không thể kiểm tra điều kiện đánh giá."
                };
            }
            catch
            {
                return new CanReviewViewModel
                {
                    CanReview = false,
                    Message = "Có lỗi xảy ra khi kiểm tra điều kiện đánh giá."
                };
            }
        }

        public async Task<ReviewViewModel?> GetMyReviewForBookAsync(int bookId, string token)
        {
            try
            {
                var apiBaseUrl = GetApiBaseUrl();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync($"{apiBaseUrl}/api/Review/my-review/{bookId}");

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var reviewData = JsonSerializer.Deserialize<dynamic>(responseContent, _jsonOptions);

                    return MapToReviewViewModel(reviewData);
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        private ReviewViewModel MapToReviewViewModel(dynamic reviewData)
        {
            return new ReviewViewModel
            {
                Id = GetPropertyValue<int>(reviewData, "id"),
                UserId = GetPropertyValue<string>(reviewData, "userId") ?? "",
                UserName = GetPropertyValue<string>(reviewData, "userName") ?? "",
                UserAvatarUrl = GetPropertyValue<string>(reviewData, "userAvatarUrl") ?? "",
                BookId = GetPropertyValue<int>(reviewData, "bookId"),
                BookTitle = GetPropertyValue<string>(reviewData, "bookTitle") ?? "",
                BookCoverUrl = GetPropertyValue<string>(reviewData, "bookCoverUrl") ?? "",
                Rating = GetPropertyValue<int>(reviewData, "rating"),
                Comment = GetPropertyValue<string>(reviewData, "comment"),
                ReviewDate = GetPropertyValue<DateTime>(reviewData, "reviewDate"),
                CreatedAt = GetPropertyValue<DateTime>(reviewData, "createdAt"),
                UpdatedAt = GetPropertyValue<DateTime?>(reviewData, "updatedAt"),
                CanEdit = GetPropertyValue<bool>(reviewData, "canEdit")
            };
        }

        private T GetPropertyValue<T>(dynamic obj, string propertyName)
        {
            try
            {
                if (obj is JsonElement element)
                {
                    if (element.TryGetProperty(propertyName, out JsonElement prop))
                    {
                        if (typeof(T) == typeof(string))
                            return (T)(object)(prop.GetString() ?? string.Empty);
                        if (typeof(T) == typeof(int))
                            return (T)(object)prop.GetInt32();
                        if (typeof(T) == typeof(double))
                            return (T)(object)prop.GetDouble();
                        if (typeof(T) == typeof(bool))
                            return (T)(object)prop.GetBoolean();
                        if (typeof(T) == typeof(DateTime))
                        {
                            if (prop.TryGetDateTime(out DateTime dateValue))
                                return (T)(object)dateValue;
                            if (DateTime.TryParse(prop.GetString(), out DateTime parsedDate))
                                return (T)(object)parsedDate;
                        }
                        if (typeof(T) == typeof(DateTime?))
                        {
                            if (prop.ValueKind == JsonValueKind.Null)
                                return default(T)!;
                            if (prop.TryGetDateTime(out DateTime dateValue))
                                return (T)(object)dateValue;
                            if (DateTime.TryParse(prop.GetString(), out DateTime parsedDate))
                                return (T)(object)parsedDate;
                        }
                        if (typeof(T) == typeof(List<dynamic>))
                        {
                            var list = new List<dynamic>();
                            foreach (var item in prop.EnumerateArray())
                            {
                                list.Add(item);
                            }
                            return (T)(object)list;
                        }
                    }
                }
                return default(T)!;
            }
            catch
            {
                return default(T)!;
            }
        }
    }
}
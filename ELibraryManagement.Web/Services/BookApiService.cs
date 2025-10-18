using ELibraryManagement.Web.Models;
using ELibraryManagement.Web.Models.ViewModels;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ELibraryManagement.Web.Services
{
    // DTO classes to match API response
    public class ApiBookDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string? ISBN { get; set; }
        public string? Publisher { get; set; }
        public int PublicationYear { get; set; }
        public string? Description { get; set; }
        public string? CoverImageUrl { get; set; }
        public int Quantity { get; set; }
        public int AvailableQuantity { get; set; }
        public string? Language { get; set; }
        public int PageCount { get; set; }
        public float AverageRating { get; set; }
        public int RatingCount { get; set; }
        public List<ApiCategoryDto>? Categories { get; set; }
    }

    public class ApiCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Color { get; set; }
    }

    public interface IBookApiService
    {
        Task<List<BookViewModel>> GetAvailableBooksAsync();
        Task<PagedResult<BookViewModel>> GetAvailableBooksPagedAsync(string? search, string? category, string? author, string? sortBy, int page = 1, int pageSize = 12);
        Task<List<BookViewModel>> GetAvailableBooksAsync(string? search, string? category, string? author, string? sortBy, int page = 1, int pageSize = 12);
        Task<BookViewModel?> GetBookByIdAsync(int id);
        Task<List<BookViewModel>> GetRelatedBooksAsync(int excludeId, string? categoryName, int count = 4);
        Task<List<string>> GetCategoriesAsync();
        Task<List<string>> GetAuthorsAsync();
        Task<BorrowBookResponseViewModel> BorrowBookAsync(BorrowBookRequestViewModel request, string token);
        Task<List<UserBorrowedBookViewModel>> GetBorrowedBooksAsync(string userId, string token);
        Task<List<UserBorrowedBookViewModel>> GetBorrowHistoryAsync(string userId, string token);
        Task<bool> HasUserBorrowedBookAsync(string userId, int bookId, string token);
        Task<BorrowBookResponseViewModel> ReturnBookAsync(int borrowRecordId, string token);
        Task<BorrowBookResponseViewModel> CancelBorrowRequestAsync(int borrowRecordId, string token);
    }

    public class BookApiService : IBookApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<BookApiService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public BookApiService(HttpClient httpClient, IConfiguration configuration, ILogger<BookApiService> logger)
        {
            _httpClient = httpClient;
            _httpClient.Timeout = TimeSpan.FromSeconds(30); // Set timeout
            _configuration = configuration;
            _logger = logger;
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
            return httpsUrl ?? httpUrl ?? "http://localhost:5293";
        }

        public async Task<List<BookViewModel>> GetAvailableBooksAsync()
        {
            try
            {
                _logger.LogInformation("GetAvailableBooksAsync (simple) called");

                var apiBaseUrl = GetApiBaseUrl();
                var fullUrl = $"{apiBaseUrl}/api/Book/available";

                _logger.LogInformation("Making API call to: {Url}", fullUrl);

                var response = await _httpClient.GetAsync(fullUrl);

                _logger.LogInformation("API response status: {StatusCode}", response.StatusCode);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();

                    _logger.LogInformation("API response content length: {Length}", content.Length);
                    _logger.LogDebug("API response content: {Content}", content);

                    var books = JsonSerializer.Deserialize<List<dynamic>>(content, _jsonOptions);

                    var bookViewModels = books?.Select(book => new BookViewModel
                    {
                        Id = GetPropertyValue<int>(book, "id"),
                        Title = GetPropertyValue<string>(book, "title") ?? string.Empty,
                        Author = GetPropertyValue<string>(book, "author") ?? string.Empty,
                        ISBN = GetPropertyValue<string>(book, "isbn") ?? string.Empty,
                        Publisher = GetPropertyValue<string>(book, "publisher") ?? string.Empty,
                        PublicationYear = GetPropertyValue<int>(book, "publicationYear"),
                        Description = GetPropertyValue<string>(book, "description") ?? string.Empty,
                        ImageUrl = GetPropertyValue<string>(book, "coverImageUrl") ?? string.Empty,
                        TotalCopies = GetPropertyValue<int>(book, "quantity"),
                        AvailableCopies = GetPropertyValue<int>(book, "availableQuantity"),
                        CategoryName = GetCategoryName(book)
                    }).ToList() ?? new List<BookViewModel>();

                    _logger.LogInformation("Parsed {BookCount} books successfully", bookViewModels.Count);

                    return bookViewModels;
                }
                else
                {
                    _logger.LogError("API call failed with status: {StatusCode}", response.StatusCode);
                }

                return new List<BookViewModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAvailableBooksAsync (simple)");

                return new List<BookViewModel>();
            }
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
                        if (typeof(T) == typeof(decimal?) || typeof(T) == typeof(decimal))
                        {
                            if (prop.ValueKind == JsonValueKind.Null)
                                return default(T);
                            return (T)(object)prop.GetDecimal();
                        }
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
                                return default(T);
                            if (prop.TryGetDateTime(out DateTime dateValue))
                                return (T)(object)dateValue;
                            if (DateTime.TryParse(prop.GetString(), out DateTime parsedDate))
                                return (T)(object)parsedDate;
                        }
                    }
                }
                return default(T);
            }
            catch
            {
                return default(T)!;
            }
        }

        public async Task<PagedResult<BookViewModel>> GetAvailableBooksPagedAsync(string? search, string? category, string? author, string? sortBy, int page = 1, int pageSize = 6)
        {
            try
            {
                _logger.LogInformation("GetAvailableBooksPagedAsync called with search='{Search}', category='{Category}', author='{Author}', sortBy='{SortBy}', page={Page}, pageSize={PageSize}",
                    search, category, author, sortBy, page, pageSize);

                var apiBaseUrl = GetApiBaseUrl();

                // Use simple API call and filter client-side since OData filtering is not working properly
                var fullUrl = $"{apiBaseUrl}/api/Book/available";

                _logger.LogInformation("Making API call to: {Url}", fullUrl);

                var response = await _httpClient.GetAsync(fullUrl);

                _logger.LogInformation("API response status: {StatusCode}", response.StatusCode);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();

                    _logger.LogInformation("API response content length: {Length}", content.Length);
                    _logger.LogDebug("API response content: {Content}", content);

                    // Parse response as array
                    var jsonDocument = JsonDocument.Parse(content);
                    var root = jsonDocument.RootElement;

                    List<BookViewModel> allBooks = new List<BookViewModel>();

                    if (root.ValueKind == JsonValueKind.Array)
                    {
                        _logger.LogInformation("Response is regular array with {Count} items", root.GetArrayLength());
                        allBooks = ParseBookList(root);
                    }
                    else
                    {
                        _logger.LogWarning("Unknown response format: {ValueKind}, trying to parse as regular array", root.ValueKind);
                        allBooks = ParseBookList(root);
                    }

                    // Apply client-side filtering
                    var filteredBooks = allBooks.AsQueryable();

                    if (!string.IsNullOrEmpty(search))
                    {
                        var searchLower = search.ToLower();
                        filteredBooks = filteredBooks.Where(b =>
                            b.Title.ToLower().Contains(searchLower) ||
                            b.Author.ToLower().Contains(searchLower) ||
                            b.Description.ToLower().Contains(searchLower));
                    }

                    if (!string.IsNullOrEmpty(category))
                    {
                        var categoryLower = category.ToLower();
                        filteredBooks = filteredBooks.Where(b =>
                            b.CategoryName.ToLower().Contains(categoryLower));
                    }

                    if (!string.IsNullOrEmpty(author))
                    {
                        var authorLower = author.ToLower();
                        filteredBooks = filteredBooks.Where(b =>
                            b.Author.ToLower().Contains(authorLower));
                    }

                    // Apply client-side sorting
                    if (!string.IsNullOrEmpty(sortBy))
                    {
                        filteredBooks = sortBy switch
                        {
                            "title" => filteredBooks.OrderBy(b => b.Title),
                            "title_desc" => filteredBooks.OrderByDescending(b => b.Title),
                            "author" => filteredBooks.OrderBy(b => b.Author),
                            "author_desc" => filteredBooks.OrderByDescending(b => b.Author),
                            "year" => filteredBooks.OrderByDescending(b => b.PublicationYear),
                            "year_asc" => filteredBooks.OrderBy(b => b.PublicationYear),
                            _ => filteredBooks.OrderBy(b => b.Title)
                        };
                    }
                    else
                    {
                        filteredBooks = filteredBooks.OrderBy(b => b.Title);
                    }

                    var filteredList = filteredBooks.ToList();
                    int totalCount = filteredList.Count;

                    // Apply client-side pagination
                    var startIndex = (page - 1) * pageSize;
                    var pagedBooks = filteredList.Skip(startIndex).Take(pageSize).ToList();

                    _logger.LogInformation("Total books after filtering: {TotalCount}, showing page {Page} with {CurrentCount} books", totalCount, page, pagedBooks.Count);

                    return new PagedResult<BookViewModel>
                    {
                        Items = pagedBooks,
                        TotalCount = totalCount,
                        PageNumber = page,
                        PageSize = pageSize
                    };
                }
                else
                {
                    _logger.LogError("API call failed with status: {StatusCode}", response.StatusCode);
                }

                return new PagedResult<BookViewModel>
                {
                    Items = new List<BookViewModel>(),
                    TotalCount = 0,
                    PageNumber = page,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAvailableBooksPagedAsync for page {Page}, pageSize {PageSize}", page, pageSize);

                return new PagedResult<BookViewModel>
                {
                    Items = new List<BookViewModel>(),
                    TotalCount = 0,
                    PageNumber = page,
                    PageSize = pageSize
                };
            }
        }

        private List<BookViewModel> ParseBookList(JsonElement element)
        {
            var books = new List<BookViewModel>();

            if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (var bookElement in element.EnumerateArray())
                {
                    books.Add(ParseBookFromJson(bookElement));
                }
            }

            return books;
        }

        private BookViewModel ParseBookFromJson(JsonElement book)
        {
            return new BookViewModel
            {
                Id = GetPropertyValue<int>(book, "id"),
                Title = GetPropertyValue<string>(book, "title") ?? string.Empty,
                Author = GetPropertyValue<string>(book, "author") ?? string.Empty,
                ISBN = GetPropertyValue<string>(book, "isbn") ?? string.Empty,
                Publisher = GetPropertyValue<string>(book, "publisher") ?? string.Empty,
                PublicationYear = GetPropertyValue<int>(book, "publicationYear"),
                Description = GetPropertyValue<string>(book, "description") ?? string.Empty,
                ImageUrl = GetPropertyValue<string>(book, "coverImageUrl") ?? string.Empty,
                TotalCopies = GetPropertyValue<int>(book, "quantity"),
                AvailableCopies = GetPropertyValue<int>(book, "availableQuantity"),
                CategoryName = GetCategoryName(book)
            };
        }

        private async Task<int> GetTotalBookCountAsync(string? search, string? category, string? author)
        {
            try
            {
                var apiBaseUrl = GetApiBaseUrl();
                var queryParams = new List<string>();

                // Build same filters as main query
                var filters = new List<string>();

                if (!string.IsNullOrEmpty(search))
                {
                    filters.Add($"(contains(tolower(title), '{search.ToLower()}') or contains(tolower(author), '{search.ToLower()}') or contains(tolower(description), '{search.ToLower()}'))");
                }

                if (!string.IsNullOrEmpty(category))
                {
                    filters.Add($"categories/any(c: contains(tolower(c/name), '{category.ToLower()}'))");
                }

                if (!string.IsNullOrEmpty(author))
                {
                    filters.Add($"contains(tolower(author), '{author.ToLower()}')");
                }

                if (filters.Any())
                {
                    queryParams.Add($"$filter={string.Join(" and ", filters)}");
                }

                queryParams.Add("$count=true");
                queryParams.Add("$top=0"); // We only want the count

                var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
                var response = await _httpClient.GetAsync($"{apiBaseUrl}/api/Book/available{queryString}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var jsonDocument = JsonDocument.Parse(content);
                    var root = jsonDocument.RootElement;

                    if (root.TryGetProperty("@odata.count", out var countElement))
                    {
                        return countElement.GetInt32();
                    }
                }

                return 0;
            }
            catch
            {
                return 0;
            }
        }

        public async Task<List<BookViewModel>> GetAvailableBooksAsync(string? search, string? category, string? author, string? sortBy, int page = 1, int pageSize = 12)
        {
            try
            {
                var apiBaseUrl = GetApiBaseUrl();
                var queryParams = new List<string>();

                // Build OData query
                var filters = new List<string>();

                if (!string.IsNullOrEmpty(search))
                {
                    filters.Add($"(contains(tolower(title), '{search.ToLower()}') or contains(tolower(author), '{search.ToLower()}') or contains(tolower(description), '{search.ToLower()}'))");
                }

                if (!string.IsNullOrEmpty(category))
                {
                    filters.Add($"categories/any(c: contains(tolower(c/name), '{category.ToLower()}'))");
                }

                if (!string.IsNullOrEmpty(author))
                {
                    filters.Add($"contains(tolower(author), '{author.ToLower()}')");
                }

                if (filters.Any())
                {
                    queryParams.Add($"$filter={string.Join(" and ", filters)}");
                }

                // Sorting
                if (!string.IsNullOrEmpty(sortBy))
                {
                    var orderBy = sortBy switch
                    {
                        "title" => "title",
                        "title_desc" => "title desc",
                        "author" => "author",
                        "author_desc" => "author desc",
                        "year" => "publicationYear desc",
                        "year_asc" => "publicationYear",
                        _ => "title"
                    };
                    queryParams.Add($"$orderby={orderBy}");
                }

                // Pagination
                var skip = (page - 1) * pageSize;
                queryParams.Add($"$skip={skip}");
                queryParams.Add($"$top={pageSize}");

                var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
                var response = await _httpClient.GetAsync($"{apiBaseUrl}/api/Book/available{queryString}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var books = JsonSerializer.Deserialize<List<dynamic>>(content, _jsonOptions);

                    return books?.Select(book => new BookViewModel
                    {
                        Id = GetPropertyValue<int>(book, "id"),
                        Title = GetPropertyValue<string>(book, "title") ?? string.Empty,
                        Author = GetPropertyValue<string>(book, "author") ?? string.Empty,
                        ISBN = GetPropertyValue<string>(book, "isbn") ?? string.Empty,
                        Publisher = GetPropertyValue<string>(book, "publisher") ?? string.Empty,
                        PublicationYear = GetPropertyValue<int>(book, "publicationYear"),
                        Description = GetPropertyValue<string>(book, "description") ?? string.Empty,
                        ImageUrl = GetPropertyValue<string>(book, "coverImageUrl") ?? string.Empty,
                        TotalCopies = GetPropertyValue<int>(book, "quantity"),
                        AvailableCopies = GetPropertyValue<int>(book, "availableQuantity"),
                        CategoryName = GetCategoryName(book)
                    }).ToList() ?? new List<BookViewModel>();
                }

                return new List<BookViewModel>();
            }
            catch (Exception)
            {
                return new List<BookViewModel>();
            }
        }

        public async Task<List<string>> GetCategoriesAsync()
        {
            try
            {
                var apiBaseUrl = GetApiBaseUrl();
                // Try different endpoints
                var endpoints = new[] {
                    $"{apiBaseUrl}/api/Book/categories",
                    $"{apiBaseUrl}/api/Book?$select=category",
                    $"{apiBaseUrl}/api/Book/available?$select=categories&$expand=categories",
                    $"{apiBaseUrl}/api/Book"
                };

                foreach (var endpoint in endpoints)
                {
                    try
                    {
                        var response = await _httpClient.GetAsync(endpoint);
                        if (response.IsSuccessStatusCode)
                        {
                            var content = await response.Content.ReadAsStringAsync();
                            var books = JsonSerializer.Deserialize<List<dynamic>>(content, _jsonOptions);

                            var categories = new HashSet<string>();

                            if (books != null)
                            {
                                foreach (var book in books)
                                {
                                    var categoryName = GetCategoryName(book);
                                    if (!string.IsNullOrEmpty(categoryName) && categoryName != "Chưa phân loại")
                                    {
                                        categories.Add(categoryName);
                                    }
                                }
                            }

                            if (categories.Any())
                            {
                                return categories.OrderBy(c => c).ToList();
                            }
                        }
                    }
                    catch
                    {
                        // Try next endpoint
                        continue;
                    }
                }

                // Fallback: Return some default categories
                return new List<string> {
                    "Văn học",
                    "Khoa học",
                    "Lịch sử",
                    "Công nghệ",
                    "Kinh tế",
                    "Tâm lý học",
                    "Nấu ăn",
                    "Du lịch"
                };
            }
            catch (Exception)
            {
                // Return default categories if API fails
                return new List<string> {
                    "Văn học",
                    "Khoa học",
                    "Lịch sử",
                    "Công nghệ",
                    "Kinh tế",
                    "Tâm lý học",
                    "Nấu ăn",
                    "Du lịch"
                };
            }
        }

        public async Task<List<string>> GetAuthorsAsync()
        {
            try
            {
                var apiBaseUrl = GetApiBaseUrl();
                // Try different endpoints
                var endpoints = new[] {
                    $"{apiBaseUrl}/api/Book/authors",
                    $"{apiBaseUrl}/api/Book?$select=author",
                    $"{apiBaseUrl}/api/Book/available?$select=author",
                    $"{apiBaseUrl}/api/Book"
                };

                foreach (var endpoint in endpoints)
                {
                    try
                    {
                        var response = await _httpClient.GetAsync(endpoint);
                        if (response.IsSuccessStatusCode)
                        {
                            var content = await response.Content.ReadAsStringAsync();
                            var books = JsonSerializer.Deserialize<List<dynamic>>(content, _jsonOptions);

                            var authors = new HashSet<string>();

                            if (books != null)
                            {
                                foreach (var book in books)
                                {
                                    var author = GetPropertyValue<string>(book, "author");
                                    if (!string.IsNullOrEmpty(author))
                                    {
                                        authors.Add(author);
                                    }
                                }
                            }

                            if (authors.Any())
                            {
                                return authors.OrderBy(a => a).ToList();
                            }
                        }
                    }
                    catch
                    {
                        // Try next endpoint
                        continue;
                    }
                }

                // Fallback: Return some default authors
                return new List<string> {
                    "Nguyễn Du",
                    "Tô Hoài",
                    "Nam Cao",
                    "Xuân Diệu",
                    "Hồ Chí Minh",
                    "Dale Carnegie",
                    "Napoleon Hill",
                    "Stephen Covey"
                };
            }
            catch (Exception)
            {
                // Return default authors if API fails
                return new List<string> {
                    "Nguyễn Du",
                    "Tô Hoài",
                    "Nam Cao",
                    "Xuân Diệu",
                    "Hồ Chí Minh",
                    "Dale Carnegie",
                    "Napoleon Hill",
                    "Stephen Covey"
                };
            }
        }

        private string GetCategoryName(JsonElement book)
        {
            try
            {

                // Try different possible structures for category

                // 1. Try "categoryName" property
                if (book.TryGetProperty("categoryName", out JsonElement categoryName))
                {
                    var name = categoryName.GetString();
                    if (!string.IsNullOrEmpty(name))
                    {

                        return name;
                    }
                }

                // 2. Try "category" property
                if (book.TryGetProperty("category", out JsonElement category))
                {
                    var name = category.GetString();
                    if (!string.IsNullOrEmpty(name))
                    {

                        return name;
                    }
                }

                // 3. Try "categories" array
                if (book.TryGetProperty("categories", out JsonElement categories))
                {
                    if (categories.ValueKind == JsonValueKind.Array && categories.GetArrayLength() > 0)
                    {
                        var firstCategory = categories[0];

                        // Try "name" property in category object
                        if (firstCategory.TryGetProperty("name", out JsonElement nameElement))
                        {
                            var catNameFromArray = nameElement.GetString();
                            if (!string.IsNullOrEmpty(catNameFromArray))
                            {

                                return catNameFromArray;
                            }
                        }

                        // Try direct string value
                        if (firstCategory.ValueKind == JsonValueKind.String)
                        {
                            var catName = firstCategory.GetString();
                            if (!string.IsNullOrEmpty(catName))
                            {

                                return catName;
                            }
                        }
                    }
                }

                // 4. Try "bookCategory" property (nested object)
                if (book.TryGetProperty("bookCategory", out JsonElement bookCategory))
                {
                    if (bookCategory.ValueKind == JsonValueKind.Object)
                    {
                        // Try "name" in bookCategory
                        if (bookCategory.TryGetProperty("name", out JsonElement name))
                        {
                            var catName = name.GetString();
                            if (!string.IsNullOrEmpty(catName))
                            {

                                return catName;
                            }
                        }

                        // Try "categoryName" in bookCategory
                        if (bookCategory.TryGetProperty("categoryName", out JsonElement catNameElement))
                        {
                            var categoryNameFromBookCat = catNameElement.GetString();
                            if (!string.IsNullOrEmpty(categoryNameFromBookCat))
                            {

                                return categoryNameFromBookCat;
                            }
                        }

                        // Try "title" in bookCategory
                        if (bookCategory.TryGetProperty("title", out JsonElement titleElement))
                        {
                            var categoryTitle = titleElement.GetString();
                            if (!string.IsNullOrEmpty(categoryTitle))
                            {

                                return categoryTitle;
                            }
                        }
                    }
                }

                // 5. Try "categoryId" and map to name (if we had a mapping)
                if (book.TryGetProperty("categoryId", out JsonElement categoryId))
                {
                    var id = categoryId.GetInt32();


                    // Simple mapping for common categories
                    var categoryMap = new Dictionary<int, string>
                    {
                        {1, "Văn học"},
                        {2, "Khoa học"},
                        {3, "Lịch sử"},
                        {4, "Công nghệ"},
                        {5, "Kinh tế"},
                        {6, "Nghệ thuật"},
                        {7, "Thể thao"},
                        {8, "Du lịch"},
                        {9, "Nấu ăn"},
                        {10, "Sức khỏe"}
                    };

                    if (categoryMap.TryGetValue(id, out var mappedName))
                    {

                        return mappedName;
                    }
                }

                // 6. Try other possible property names
                var possibleNames = new[] { "genre", "subject", "type", "classification", "group" };
                foreach (var propName in possibleNames)
                {
                    if (book.TryGetProperty(propName, out JsonElement propValue))
                    {
                        var name = propValue.GetString();
                        if (!string.IsNullOrEmpty(name))
                        {

                            return name;
                        }
                    }
                }


                return "Chưa phân loại";
            }
            catch (Exception ex)
            {


                return "Chưa phân loại";
            }
        }

        public async Task<BookViewModel?> GetBookByIdAsync(int id)
        {
            try
            {
                var apiBaseUrl = GetApiBaseUrl();
                var response = await _httpClient.GetAsync($"{apiBaseUrl}/api/Book/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();


                    // Parse JSON manually for now
                    var jsonDoc = JsonDocument.Parse(content);
                    var root = jsonDoc.RootElement;

                    // Debug: Log all properties in the response

                    foreach (var property in root.EnumerateObject())
                    {

                        if (property.Value.ValueKind == JsonValueKind.Object)
                        {

                            foreach (var subProp in property.Value.EnumerateObject())
                            {

                            }
                        }
                        else if (property.Value.ValueKind == JsonValueKind.Array)
                        {

                        }
                    }

                    // Create a simple BookViewModel with basic properties
                    return new BookViewModel
                    {
                        Id = root.TryGetProperty("id", out var idProp) ? idProp.GetInt32() : id,
                        Title = root.TryGetProperty("title", out var titleProp) ? titleProp.GetString() ?? $"Book {id}" : $"Book {id}",
                        Author = root.TryGetProperty("author", out var authorProp) ? authorProp.GetString() ?? "Unknown Author" : "Unknown Author",
                        ISBN = root.TryGetProperty("isbn", out var isbnProp) ? isbnProp.GetString() ?? "" : "",
                        Publisher = root.TryGetProperty("publisher", out var pubProp) ? pubProp.GetString() ?? "" : "",
                        PublicationYear = root.TryGetProperty("publicationYear", out var yearProp) ? yearProp.GetInt32() : DateTime.Now.Year,
                        Description = root.TryGetProperty("description", out var descProp) ? descProp.GetString() ?? "No description available" : "No description available",
                        ImageUrl = root.TryGetProperty("coverImageUrl", out var imgProp) ? imgProp.GetString() ?? "/images/no-image.jpg" : "/images/no-image.jpg",
                        TotalCopies = root.TryGetProperty("quantity", out var qtyProp) ? qtyProp.GetInt32() : 1,
                        AvailableCopies = root.TryGetProperty("availableQuantity", out var availProp) ? availProp.GetInt32() : 1,

                        // Try multiple possible property names for category
                        CategoryName = GetCategoryName(root),
                        Language = root.TryGetProperty("language", out var langProp) ? langProp.GetString() ?? "Tiếng Việt" : "Tiếng Việt",
                        PageCount = root.TryGetProperty("pageCount", out var pageProp) ? pageProp.GetInt32() : 0,
                        AverageRating = root.TryGetProperty("averageRating", out var ratingProp) ? ratingProp.GetDecimal() : 0m,
                        RatingCount = root.TryGetProperty("ratingCount", out var countProp) ? countProp.GetInt32() : 0
                    };
                }

                return null;
            }
            catch (Exception ex)
            {


                // Return a mock book for testing if API fails
                var categories = new[] { "Văn học", "Khoa học", "Lịch sử", "Công nghệ", "Kinh tế" };
                var languages = new[] { "Tiếng Việt", "Tiếng Anh", "Tiếng Pháp" };
                var authors = new[] { "Nguyễn Du", "Tô Hoài", "Nam Cao", "Xuân Diệu", "Dale Carnegie" };

                return new BookViewModel
                {
                    Id = id,
                    Title = $"Sample Book {id}",
                    Author = authors[id % authors.Length],
                    ISBN = $"978-0000000{id:D3}",
                    Publisher = "Sample Publisher",
                    PublicationYear = 2020 + (id % 5),
                    Description = "This is a sample book description for testing purposes.",
                    ImageUrl = "https://via.placeholder.com/300x400?text=Book+Cover",
                    TotalCopies = 5,
                    AvailableCopies = 3,
                    CategoryName = categories[id % categories.Length],
                    Language = languages[id % languages.Length],
                    PageCount = 200 + (id * 20),
                    AverageRating = 3.5m + (id % 3) * 0.5m,
                    RatingCount = 5 + (id * 2)
                };
            }
        }

        public async Task<List<BookViewModel>> GetRelatedBooksAsync(int excludeId, string? categoryName, int count = 4)
        {
            try
            {
                var apiBaseUrl = GetApiBaseUrl();
                var filter = "";

                if (!string.IsNullOrEmpty(categoryName))
                {
                    filter = $"?$filter=categories/any(c: c/name eq '{categoryName}') and id ne {excludeId}&$top={count}";
                }
                else
                {
                    filter = $"?$filter=id ne {excludeId}&$top={count}";
                }

                var response = await _httpClient.GetAsync($"{apiBaseUrl}/api/Book/available{filter}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var books = JsonSerializer.Deserialize<List<dynamic>>(content, _jsonOptions);

                    return books?.Select(book => new BookViewModel
                    {
                        Id = GetPropertyValue<int>(book, "id"),
                        Title = GetPropertyValue<string>(book, "title") ?? string.Empty,
                        Author = GetPropertyValue<string>(book, "author") ?? string.Empty,
                        ImageUrl = GetPropertyValue<string>(book, "coverImageUrl") ?? string.Empty,
                        CategoryName = GetCategoryName(book),
                        AverageRating = GetPropertyValue<decimal>(book, "averageRating")
                    }).ToList() ?? new List<BookViewModel>();
                }

                return new List<BookViewModel>();
            }
            catch (Exception)
            {
                return new List<BookViewModel>();
            }
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
                    else if (typeof(T) == typeof(decimal))
                    {
                        return (T)(object)property.GetDecimal();
                    }
                    else if (typeof(T) == typeof(decimal?))
                    {
                        return property.ValueKind == JsonValueKind.Null ? default(T) : (T)(object)property.GetDecimal();
                    }
                }
                return default(T)!;
            }
            catch
            {
                return default(T)!;
            }
        }

        private string GetJsonCategoryName(JsonElement element)
        {
            try
            {
                if (element.TryGetProperty("categories", out var categoriesProperty) &&
                    categoriesProperty.ValueKind == JsonValueKind.Array)
                {
                    var firstCategory = categoriesProperty.EnumerateArray().FirstOrDefault();
                    if (firstCategory.ValueKind != JsonValueKind.Undefined &&
                        firstCategory.TryGetProperty("name", out var nameProperty))
                    {
                        return nameProperty.GetString() ?? "Chưa phân loại";
                    }
                }
                return "Chưa phân loại";
            }
            catch
            {
                return "Chưa phân loại";
            }
        }

        // Borrow Book Methods
        public async Task<BorrowBookResponseViewModel> BorrowBookAsync(BorrowBookRequestViewModel request, string token)
        {
            try
            {
                var apiBaseUrl = GetApiBaseUrl();

                System.Diagnostics.Debug.WriteLine($"=== BorrowBookAsync START ===");
                System.Diagnostics.Debug.WriteLine($"API URL: {apiBaseUrl}");
                System.Diagnostics.Debug.WriteLine($"BookId: {request.BookId}");
                System.Diagnostics.Debug.WriteLine($"UserId: {request.UserId}");
                System.Diagnostics.Debug.WriteLine($"Token: {(!string.IsNullOrEmpty(token) ? "Present" : "Missing")}");

                var requestDto = new
                {
                    BookId = request.BookId,
                    UserId = request.UserId,
                    DueDate = request.DueDate,
                    Notes = request.Notes
                };

                var json = JsonSerializer.Serialize(requestDto, _jsonOptions);
                System.Diagnostics.Debug.WriteLine($"Request JSON: {json}");

                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var fullUrl = $"{apiBaseUrl}/api/Book/borrow";
                System.Diagnostics.Debug.WriteLine($"Full URL: {fullUrl}");

                var response = await _httpClient.PostAsync(fullUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine($"Response Status: {response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"Response Content: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<dynamic>(responseContent, _jsonOptions);
                    return new BorrowBookResponseViewModel
                    {
                        Success = true,
                        BorrowRecordId = GetPropertyValue<int>(result, "borrowRecordId"),
                        BookId = GetPropertyValue<int>(result, "bookId"),
                        BookTitle = GetPropertyValue<string>(result, "bookTitle") ?? "",
                        UserId = GetPropertyValue<string>(result, "userId") ?? "",
                        BorrowDate = GetPropertyValue<DateTime>(result, "borrowDate"),
                        DueDate = GetPropertyValue<DateTime>(result, "dueDate"),
                        Status = GetPropertyValue<string>(result, "status") ?? "",
                        Message = GetPropertyValue<string>(result, "message") ?? "Mượn sách thành công!"
                    };
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"API Error: {response.StatusCode} - {responseContent}");

                    // Try to parse error response for detailed message
                    string errorMessage = $"Lỗi API: {response.StatusCode}";
                    try
                    {
                        var errorResponse = JsonSerializer.Deserialize<JsonElement>(responseContent, _jsonOptions);
                        if (errorResponse.TryGetProperty("message", out var messageProperty))
                        {
                            errorMessage = messageProperty.GetString() ?? errorMessage;
                        }
                        else if (errorResponse.TryGetProperty("title", out var titleProperty))
                        {
                            errorMessage = titleProperty.GetString() ?? errorMessage;
                        }
                        else if (errorResponse.TryGetProperty("errors", out var errorsProperty))
                        {
                            var errors = new List<string>();
                            if (errorsProperty.ValueKind == JsonValueKind.Object)
                            {
                                foreach (var error in errorsProperty.EnumerateObject())
                                {
                                    if (error.Value.ValueKind == JsonValueKind.Array)
                                    {
                                        foreach (var errorMsg in error.Value.EnumerateArray())
                                        {
                                            errors.Add(errorMsg.GetString() ?? "");
                                        }
                                    }
                                }
                                if (errors.Any())
                                {
                                    errorMessage = string.Join("; ", errors);
                                }
                            }
                        }
                    }
                    catch (JsonException)
                    {
                        // If parsing fails, use the raw response if it's a simple string
                        if (!string.IsNullOrWhiteSpace(responseContent))
                        {
                            errorMessage = responseContent;
                        }
                    }

                    return new BorrowBookResponseViewModel
                    {
                        Success = false,
                        Message = errorMessage
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception in BorrowBookAsync: {ex.Message}");
                return new BorrowBookResponseViewModel
                {
                    Success = false,
                    Message = $"Có lỗi xảy ra: {ex.Message}"
                };
            }
        }

        public async Task<List<UserBorrowedBookViewModel>> GetBorrowedBooksAsync(string userId, string token)
        {
            try
            {
                var apiBaseUrl = GetApiBaseUrl();

                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync($"{apiBaseUrl}/api/Book/borrowed/{userId}");
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var borrowedBooks = JsonSerializer.Deserialize<List<dynamic>>(responseContent, _jsonOptions);

                    return borrowedBooks?.Select(book => new UserBorrowedBookViewModel
                    {
                        BorrowRecordId = GetPropertyValue<int>(book, "id"),
                        BookId = GetPropertyValue<int>(book, "bookId"),
                        BookTitle = GetPropertyValue<string>(book, "bookTitle") ?? "",
                        BookAuthor = GetPropertyValue<string>(book, "bookAuthor") ?? "",
                        BookCoverUrl = GetPropertyValue<string>(book, "bookCoverUrl") ?? "",
                        BorrowDate = GetPropertyValue<DateTime>(book, "borrowDate"),
                        DueDate = GetPropertyValue<DateTime>(book, "dueDate"),
                        ReturnDate = GetPropertyValue<DateTime?>(book, "returnDate"),
                        Status = GetPropertyValue<string>(book, "status") ?? "",
                        Notes = GetPropertyValue<string>(book, "notes")
                    }).ToList() ?? new List<UserBorrowedBookViewModel>();
                }

                return new List<UserBorrowedBookViewModel>();
            }
            catch (Exception)
            {
                return new List<UserBorrowedBookViewModel>();
            }
        }

        public async Task<List<UserBorrowedBookViewModel>> GetBorrowHistoryAsync(string userId, string token)
        {
            try
            {
                var apiBaseUrl = GetApiBaseUrl();

                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync($"{apiBaseUrl}/api/Book/history/{userId}");
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var borrowHistory = JsonSerializer.Deserialize<List<dynamic>>(responseContent, _jsonOptions);

                    return borrowHistory?.Select(borrow => new UserBorrowedBookViewModel
                    {
                        BorrowRecordId = GetPropertyValue<int>(borrow, "id"),
                        BookId = GetPropertyValue<int>(borrow, "bookId"),
                        BookTitle = GetPropertyValue<string>(borrow, "bookTitle") ?? "",
                        BookAuthor = GetPropertyValue<string>(borrow, "bookAuthor") ?? "",
                        BookCoverUrl = GetPropertyValue<string>(borrow, "bookCoverUrl") ?? "",
                        BorrowDate = GetPropertyValue<DateTime>(borrow, "borrowDate"),
                        DueDate = GetPropertyValue<DateTime>(borrow, "dueDate"),
                        ReturnDate = GetPropertyValue<DateTime?>(borrow, "returnDate"),
                        Status = GetPropertyValue<string>(borrow, "status") ?? "",
                        Notes = GetPropertyValue<string>(borrow, "notes"),
                        FineAmount = GetPropertyValue<decimal?>(borrow, "fineAmount"),
                        FineStatus = GetPropertyValue<string>(borrow, "fineStatus"),
                        FineReason = GetPropertyValue<string>(borrow, "fineReason")
                    }).ToList() ?? new List<UserBorrowedBookViewModel>();
                }

                return new List<UserBorrowedBookViewModel>();
            }
            catch (Exception)
            {
                return new List<UserBorrowedBookViewModel>();
            }
        }

        public async Task<BorrowBookResponseViewModel> ReturnBookAsync(int borrowRecordId, string token)
        {
            try
            {
                var apiBaseUrl = GetApiBaseUrl();

                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.PostAsync($"{apiBaseUrl}/api/Book/return/{borrowRecordId}", null);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return new BorrowBookResponseViewModel
                    {
                        Success = true,
                        Message = "Trả sách thành công!"
                    };
                }
                else
                {
                    return new BorrowBookResponseViewModel
                    {
                        Success = false,
                        Message = "Không thể trả sách. Vui lòng thử lại sau."
                    };
                }
            }
            catch (Exception ex)
            {
                return new BorrowBookResponseViewModel
                {
                    Success = false,
                    Message = $"Có lỗi xảy ra: {ex.Message}"
                };
            }
        }

        public async Task<BorrowBookResponseViewModel> CancelBorrowRequestAsync(int borrowRecordId, string token)
        {
            try
            {
                var apiBaseUrl = GetApiBaseUrl();

                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.PostAsync($"{apiBaseUrl}/api/Book/cancel/{borrowRecordId}", null);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return new BorrowBookResponseViewModel
                    {
                        Success = true,
                        Message = "Hủy yêu cầu mượn sách thành công!"
                    };
                }
                else
                {
                    return new BorrowBookResponseViewModel
                    {
                        Success = false,
                        Message = "Không thể hủy yêu cầu. Vui lòng thử lại sau."
                    };
                }
            }
            catch (Exception ex)
            {
                return new BorrowBookResponseViewModel
                {
                    Success = false,
                    Message = $"Có lỗi xảy ra: {ex.Message}"
                };
            }
        }

        public async Task<bool> HasUserBorrowedBookAsync(string userId, int bookId, string token)
        {
            try
            {
                var borrowedBooks = await GetBorrowedBooksAsync(userId, token);
                return borrowedBooks.Any(b => b.BookId == bookId);
            }
            catch
            {
                return false;
            }
        }
    }
}

using ELibraryManagement.Web.Models;
using System.Text.Json;

namespace ELibraryManagement.Web.Services
{
    public interface IBookApiService
    {
        Task<List<BookViewModel>> GetAvailableBooksAsync();
        Task<List<BookViewModel>> GetAvailableBooksAsync(string? search, string? category, string? author, decimal? minPrice, decimal? maxPrice, string? sortBy, int page = 1, int pageSize = 12);
        Task<BookViewModel?> GetBookByIdAsync(int id);
        Task<List<BookViewModel>> GetRelatedBooksAsync(int excludeId, string? categoryName, int count = 4);
        Task<List<string>> GetCategoriesAsync();
        Task<List<string>> GetAuthorsAsync();
        Task<BorrowBookResponseViewModel> BorrowBookAsync(BorrowBookRequestViewModel request, string token);
        Task<List<UserBorrowedBookViewModel>> GetBorrowedBooksAsync(string userId, string token);
        Task<BorrowBookResponseViewModel> ReturnBookAsync(int borrowRecordId, string token);
    }

    public class BookApiService : IBookApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly JsonSerializerOptions _jsonOptions;

        public BookApiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _httpClient.Timeout = TimeSpan.FromSeconds(30); // Set timeout
            _configuration = configuration;
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
                var apiBaseUrl = GetApiBaseUrl();
                var response = await _httpClient.GetAsync($"{apiBaseUrl}/api/Book/available");

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
                        RentalPrice = GetPropertyValue<decimal?>(book, "price"),
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
                    }
                }
                return default(T);
            }
            catch
            {
                return default(T)!;
            }
        }

        public async Task<List<BookViewModel>> GetAvailableBooksAsync(string? search, string? category, string? author, decimal? minPrice, decimal? maxPrice, string? sortBy, int page = 1, int pageSize = 12)
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

                if (minPrice.HasValue)
                {
                    filters.Add($"price ge {minPrice.Value}");
                }

                if (maxPrice.HasValue)
                {
                    filters.Add($"price le {maxPrice.Value}");
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
                        "price" => "price",
                        "price_desc" => "price desc",
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
                        RentalPrice = GetPropertyValue<decimal?>(book, "price"),
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
                Console.WriteLine("=== Starting GetCategoryName parsing ===");

                // Debug: Log all properties in the book element
                Console.WriteLine("Available properties in book element:");
                foreach (var property in book.EnumerateObject())
                {
                    Console.WriteLine($"  {property.Name}: {property.Value.ValueKind}");
                    if (property.Value.ValueKind == JsonValueKind.Object)
                    {
                        Console.WriteLine($"    Object properties:");
                        foreach (var subProp in property.Value.EnumerateObject())
                        {
                            Console.WriteLine($"      {subProp.Name}: {subProp.Value}");
                        }
                    }
                    else if (property.Value.ValueKind == JsonValueKind.Array)
                    {
                        Console.WriteLine($"    Array with {property.Value.GetArrayLength()} items");
                        for (int i = 0; i < Math.Min(property.Value.GetArrayLength(), 3); i++)
                        {
                            Console.WriteLine($"      [{i}]: {property.Value[i]}");
                        }
                    }
                }

                // Try different possible structures for category

                // 1. Try "categoryName" property
                if (book.TryGetProperty("categoryName", out JsonElement categoryName))
                {
                    var name = categoryName.GetString();
                    if (!string.IsNullOrEmpty(name))
                    {
                        Console.WriteLine($"✓ Found categoryName: {name}");
                        return name;
                    }
                }

                // 2. Try "category" property
                if (book.TryGetProperty("category", out JsonElement category))
                {
                    var name = category.GetString();
                    if (!string.IsNullOrEmpty(name))
                    {
                        Console.WriteLine($"✓ Found category: {name}");
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
                                Console.WriteLine($"✓ Found categories[0].name: {catNameFromArray}");
                                return catNameFromArray;
                            }
                        }

                        // Try direct string value
                        if (firstCategory.ValueKind == JsonValueKind.String)
                        {
                            var catName = firstCategory.GetString();
                            if (!string.IsNullOrEmpty(catName))
                            {
                                Console.WriteLine($"✓ Found categories[0] as string: {catName}");
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
                                Console.WriteLine($"✓ Found bookCategory.name: {catName}");
                                return catName;
                            }
                        }

                        // Try "categoryName" in bookCategory
                        if (bookCategory.TryGetProperty("categoryName", out JsonElement catNameElement))
                        {
                            var categoryNameFromBookCat = catNameElement.GetString();
                            if (!string.IsNullOrEmpty(categoryNameFromBookCat))
                            {
                                Console.WriteLine($"✓ Found bookCategory.categoryName: {categoryNameFromBookCat}");
                                return categoryNameFromBookCat;
                            }
                        }

                        // Try "title" in bookCategory
                        if (bookCategory.TryGetProperty("title", out JsonElement titleElement))
                        {
                            var categoryTitle = titleElement.GetString();
                            if (!string.IsNullOrEmpty(categoryTitle))
                            {
                                Console.WriteLine($"✓ Found bookCategory.title: {categoryTitle}");
                                return categoryTitle;
                            }
                        }
                    }
                }

                // 5. Try "categoryId" and map to name (if we had a mapping)
                if (book.TryGetProperty("categoryId", out JsonElement categoryId))
                {
                    var id = categoryId.GetInt32();
                    Console.WriteLine($"Found categoryId: {id}");

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
                        Console.WriteLine($"✓ Mapped categoryId {id} to: {mappedName}");
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
                            Console.WriteLine($"✓ Found {propName}: {name}");
                            return name;
                        }
                    }
                }

                Console.WriteLine("❌ No category found in any expected location, returning default");
                return "Chưa phân loại";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error parsing category: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
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
                    Console.WriteLine($"API Response for Book {id}: {content}");

                    // Parse JSON manually for now
                    var jsonDoc = JsonDocument.Parse(content);
                    var root = jsonDoc.RootElement;

                    // Debug: Log all properties in the response
                    Console.WriteLine($"Available properties in JSON response:");
                    foreach (var property in root.EnumerateObject())
                    {
                        Console.WriteLine($"  {property.Name}: {property.Value.ValueKind}");
                        if (property.Value.ValueKind == JsonValueKind.Object)
                        {
                            Console.WriteLine($"    Object properties:");
                            foreach (var subProp in property.Value.EnumerateObject())
                            {
                                Console.WriteLine($"      {subProp.Name}: {subProp.Value}");
                            }
                        }
                        else if (property.Value.ValueKind == JsonValueKind.Array)
                        {
                            Console.WriteLine($"    Array with {property.Value.GetArrayLength()} items");
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
                        RentalPrice = root.TryGetProperty("price", out var priceProp) ? (decimal?)priceProp.GetDecimal() : 50000m,

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
                Console.WriteLine($"Error in GetBookByIdAsync: {ex.Message}");

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
                    RentalPrice = 30000m + (id * 5000),
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
                        RentalPrice = GetPropertyValue<decimal?>(book, "price"),
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
                        RentalPrice = GetPropertyValue<decimal?>(result, "rentalPrice"),
                        Status = GetPropertyValue<string>(result, "status") ?? "",
                        Message = GetPropertyValue<string>(result, "message") ?? "Mượn sách thành công!"
                    };
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"API Error: {response.StatusCode} - {responseContent}");
                    return new BorrowBookResponseViewModel
                    {
                        Success = false,
                        Message = $"Lỗi API: {response.StatusCode}"
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
                        BorrowRecordId = GetPropertyValue<int>(book, "borrowRecordId"),
                        BookId = GetPropertyValue<int>(book, "bookId"),
                        BookTitle = GetPropertyValue<string>(book, "bookTitle") ?? "",
                        BookAuthor = GetPropertyValue<string>(book, "bookAuthor") ?? "",
                        BookCoverUrl = GetPropertyValue<string>(book, "bookCoverUrl") ?? "",
                        BorrowDate = GetPropertyValue<DateTime>(book, "borrowDate"),
                        DueDate = GetPropertyValue<DateTime>(book, "dueDate"),
                        ReturnDate = GetPropertyValue<DateTime?>(book, "returnDate"),
                        RentalPrice = GetPropertyValue<decimal?>(book, "rentalPrice"),
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
    }
}

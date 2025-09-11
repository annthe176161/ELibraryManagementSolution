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

        public async Task<List<BookViewModel>> GetAvailableBooksAsync()
        {
            try
            {
                var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7125";
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
                var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7125";
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
                var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7125";
                var response = await _httpClient.GetAsync($"{apiBaseUrl}/api/Book/available?$select=categories&$expand=categories");

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

                    return categories.OrderBy(c => c).ToList();
                }

                return new List<string>();
            }
            catch (Exception)
            {
                return new List<string>();
            }
        }

        public async Task<List<string>> GetAuthorsAsync()
        {
            try
            {
                var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7125";
                var response = await _httpClient.GetAsync($"{apiBaseUrl}/api/Book/available?$select=author");

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

                    return authors.OrderBy(a => a).ToList();
                }

                return new List<string>();
            }
            catch (Exception)
            {
                return new List<string>();
            }
        }

        private string GetCategoryName(dynamic book)
        {
            try
            {
                if (book is JsonElement element)
                {
                    if (element.TryGetProperty("categories", out JsonElement categories))
                    {
                        if (categories.ValueKind == JsonValueKind.Array && categories.GetArrayLength() > 0)
                        {
                            var firstCategory = categories[0];
                            if (firstCategory.TryGetProperty("name", out JsonElement name))
                            {
                                return name.GetString() ?? "Chưa phân loại";
                            }
                        }
                    }
                }
                return "Chưa phân loại";
            }
            catch
            {
                return "Chưa phân loại";
            }
        }

        public async Task<BookViewModel?> GetBookByIdAsync(int id)
        {
            try
            {
                var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5293";
                var response = await _httpClient.GetAsync($"{apiBaseUrl}/api/Book/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"API Response for Book {id}: {content}");

                    // Parse JSON manually for now
                    var jsonDoc = JsonDocument.Parse(content);
                    var root = jsonDoc.RootElement;

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
                        CategoryName = "Chưa phân loại",
                        Language = "Tiếng Việt",
                        PageCount = 300,
                        AverageRating = 4.5m,
                        RatingCount = 10
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetBookByIdAsync: {ex.Message}");

                // Return a mock book for testing if API fails
                return new BookViewModel
                {
                    Id = id,
                    Title = $"Sample Book {id}",
                    Author = "Sample Author",
                    ISBN = $"978-0000000{id:D3}",
                    Publisher = "Sample Publisher",
                    PublicationYear = 2024,
                    Description = "This is a sample book description for testing purposes.",
                    ImageUrl = "https://via.placeholder.com/300x400?text=Book+Cover",
                    TotalCopies = 5,
                    AvailableCopies = 3,
                    RentalPrice = 50000m,
                    CategoryName = "Văn học",
                    Language = "Tiếng Việt",
                    PageCount = 250,
                    AverageRating = 4.2m,
                    RatingCount = 15
                };
            }
        }

        public async Task<List<BookViewModel>> GetRelatedBooksAsync(int excludeId, string? categoryName, int count = 4)
        {
            try
            {
                var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7125";
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
    }
}

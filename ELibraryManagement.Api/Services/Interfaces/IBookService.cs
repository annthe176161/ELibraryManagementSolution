using ELibraryManagement.Api.DTOs;

namespace ELibraryManagement.Api.Services.Interfaces
{
    public interface IBookService
    {
        Task<IEnumerable<BookDto>> GetAvailableBooksAsync();
        Task<BookDto?> GetBookByIdAsync(int id);
    }
}

using ELibraryManagement.Api.DTOs;

namespace ELibraryManagement.Api.Services.Interfaces
{
    public interface IBookService
    {
        IQueryable<BookDto> GetAvailableBooksQueryable();
        Task<BookDto?> GetBookByIdAsync(int id);
    }
}

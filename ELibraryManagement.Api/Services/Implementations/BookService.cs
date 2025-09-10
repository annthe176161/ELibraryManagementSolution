using ELibraryManagement.Api.Data;
using ELibraryManagement.Api.DTOs;
using ELibraryManagement.Api.Models;
using ELibraryManagement.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ELibraryManagement.Api.Services.Implementations
{
    public class BookService : IBookService
    {
        private readonly ApplicationDbContext _context;

        public BookService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BookDto>> GetAvailableBooksAsync()
        {
            var books = await _context.Books
                .Where(b => b.AvailableQuantity > 0 && !b.IsDeleted)
                .Include(b => b.BookCategories)
                .ThenInclude(bc => bc.Category)
                .ToListAsync();

            return books.Select(MapToDto);
        }

        public async Task<BookDto?> GetBookByIdAsync(int id)
        {
            var book = await _context.Books
                .Where(b => b.Id == id && !b.IsDeleted)
                .Include(b => b.BookCategories)
                .ThenInclude(bc => bc.Category)
                .FirstOrDefaultAsync();

            return book != null ? MapToDto(book) : null;
        }

        private static BookDto MapToDto(Book book)
        {
            return new BookDto
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                ISBN = book.ISBN,
                Publisher = book.Publisher,
                PublicationYear = book.PublicationYear,
                Description = book.Description,
                CoverImageUrl = book.CoverImageUrl,
                Quantity = book.Quantity,
                AvailableQuantity = book.AvailableQuantity,
                Price = book.Price,
                Language = book.Language,
                PageCount = book.PageCount,
                AverageRating = (float)book.AverageRating,
                RatingCount = book.RatingCount,
                Categories = book.BookCategories?.Select(bc => new CategoryDto
                {
                    Id = bc.Category.Id,
                    Name = bc.Category.Name,
                    Description = bc.Category.Description,
                    Color = bc.Category.Color
                }).ToList()
            };
        }
    }
}

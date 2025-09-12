using ELibraryManagement.Api.DTOs;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using System.Text;

namespace ELibraryManagement.Api.Formatters
{
    public class CsvOutputFormatter : TextOutputFormatter
    {
        public CsvOutputFormatter()
        {
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/csv"));
            SupportedEncodings.Add(Encoding.UTF8);
        }

        protected override bool CanWriteType(Type? type)
        {
            return typeof(IEnumerable<BookDto>).IsAssignableFrom(type) || type == typeof(BookDto);
        }

        public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            var response = context.HttpContext.Response;
            var buffer = new StringBuilder();

            if (context.Object is IEnumerable<BookDto> books)
            {
                // Write CSV header
                buffer.AppendLine("Id,Title,Author,ISBN,Publisher,PublicationYear,Description,CoverImageUrl,Quantity,AvailableQuantity,Language,PageCount,AverageRating,RatingCount,Categories");

                foreach (var book in books)
                {
                    buffer.AppendLine(FormatBookAsCsv(book));
                }
            }
            else if (context.Object is BookDto book)
            {
                buffer.AppendLine("Id,Title,Author,ISBN,Publisher,PublicationYear,Description,CoverImageUrl,Quantity,AvailableQuantity,Language,PageCount,AverageRating,RatingCount,Categories");
                buffer.AppendLine(FormatBookAsCsv(book));
            }

            await response.WriteAsync(buffer.ToString(), selectedEncoding);
        }

        private string FormatBookAsCsv(BookDto book)
        {
            var categories = book.Categories != null
                ? string.Join(";", book.Categories.Select(c => c.Name))
                : "";

            return $"{EscapeCsv(book.Id)}," +
                   $"{EscapeCsv(book.Title)}," +
                   $"{EscapeCsv(book.Author)}," +
                   $"{EscapeCsv(book.ISBN)}," +
                   $"{EscapeCsv(book.Publisher)}," +
                   $"{EscapeCsv(book.PublicationYear)}," +
                   $"{EscapeCsv(book.Description)}," +
                   $"{EscapeCsv(book.CoverImageUrl)}," +
                   $"{EscapeCsv(book.Quantity)}," +
                   $"{EscapeCsv(book.AvailableQuantity)}," +
                   $"{EscapeCsv(book.Language)}," +
                   $"{EscapeCsv(book.PageCount)}," +
                   $"{EscapeCsv(book.AverageRating)}," +
                   $"{EscapeCsv(book.RatingCount)}," +
                   $"{EscapeCsv(categories)}";
        }

        private string EscapeCsv(object? value)
        {
            if (value == null) return "";

            var str = value.ToString();
            if (str != null && (str.Contains(",") || str.Contains("\"") || str.Contains("\n")))
            {
                return $"\"{str.Replace("\"", "\"\"")}\"";
            }
            return str ?? "";
        }
    }
}

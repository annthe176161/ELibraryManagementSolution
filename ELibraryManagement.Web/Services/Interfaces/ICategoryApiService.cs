using ELibraryManagement.Web.Models.DTOs.CategoryDtos;
using Newtonsoft.Json;
using System.Text;

namespace ELibraryManagement.Web.Services.Interfaces
{
    public interface ICategoryApiService
    {
        Task<CategoriesListResponseDto> GetAllCategoriesAsync(bool includeInactive = false);
        Task<CategoryResponseDto> GetCategoryByIdAsync(int id);
        Task<CategoryResponseDto> CreateCategoryAsync(CreateCategoryDto createDto);
        Task<CategoryResponseDto> UpdateCategoryAsync(int id, UpdateCategoryDto updateDto);
        Task<CategoryResponseDto> DeleteCategoryAsync(int id);
        Task<CategoryResponseDto> ToggleCategoryStatusAsync(int id);
        Task<bool> CheckCategoryNameExistsAsync(string name, int? excludeId = null);
    }
}
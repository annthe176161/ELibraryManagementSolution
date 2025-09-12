using ELibraryManagement.Api.DTOs;

namespace ELibraryManagement.Api.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<CategoriesListResponseDto> GetAllCategoriesAsync(bool includeInactive = false);
        Task<CategoryResponseDto> GetCategoryByIdAsync(int id);
        Task<CategoryResponseDto> CreateCategoryAsync(CreateCategoryDto createDto);
        Task<CategoryResponseDto> UpdateCategoryAsync(int id, UpdateCategoryDto updateDto);
        Task<CategoryResponseDto> DeleteCategoryAsync(int id);
        Task<CategoryResponseDto> ToggleCategoryStatusAsync(int id);
        Task<bool> CategoryExistsAsync(string name, int? excludeId = null);
    }
}
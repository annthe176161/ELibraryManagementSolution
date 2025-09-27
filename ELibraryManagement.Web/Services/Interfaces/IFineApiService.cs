using ELibraryManagement.Web.Models;
using ELibraryManagement.Web.Models.ViewModels;

namespace ELibraryManagement.Web.Services.Interfaces
{
    public interface IFineApiService
    {
        void SetAuthToken(string token);
        Task<(List<FineViewModel> fines, int totalCount, int totalPages)> GetAllFinesAsync(int page = 1, int pageSize = 20, string? status = null, string? search = null);
        Task<FineDetailViewModel?> GetFineDetailsAsync(int id);
        Task<bool> CreateFineAsync(CreateFineRequest request);
        Task<bool> UpdateFineAsync(int id, UpdateFineRequest request);
        Task<bool> MarkFineAsPaidAsync(int id, string? notes = null);
        Task<bool> WaiveFineAsync(int id, string reason, string? notes = null);
        Task<List<FineViewModel>> GetUserFinesAsync(string userId);
        Task<FineStatisticsViewModel> GetFineStatisticsAsync();
    }
}

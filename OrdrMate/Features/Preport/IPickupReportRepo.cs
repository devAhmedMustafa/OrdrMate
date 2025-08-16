using OrdrMate.Models;
namespace OrdrMate.Repositories
{
    public interface IPickupReportRepo
    {
            Task AddReportAsync(PickupReport report);
            Task<PickupReport> GetReportById(string reportId);
            Task UpdateReportAsync(PickupReport report);
    
        Task<bool> ReportPickupAsync(PickupReportDto report);
        Task<bool> CancelReportAsync(string reportId);
        Task<bool> ApprovePickupAsync(string reportId);

    }
}
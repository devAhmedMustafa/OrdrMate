using OrdrMate.Models;
using OrdrMate.Repositories;
using OrdrMate.DTOs;
using OrdrMate.Enums;
namespace OrdrMate.Services
{
    public class PickupReportService
    {
        private readonly IPickupReportRepo _pickupReportRepo;
        private readonly IOrderRepo _orderRepo;

        public PickupReportService(IPickupReportRepo pickupReportRepo, IOrderRepo orderRepo)
        {
            _pickupReportRepo = pickupReportRepo;
            _orderRepo = orderRepo;
        }

        public async Task<bool> ReportPickupAsync(PickupReportDto report)
        {
            var order = await _orderRepo.GetOrderById(report.OrderId);
            if (order == null || order.Status != OrderStatus.Ready)
                return false;

            var currentTime = DateTime.UtcNow;
            var reportTime = currentTime - order.ReadyTime;

            if (reportTime.TotalMinutes > 30)
            {
                return false;  
            }

            var newReport = new PickupReport
            {
                OrderId = report.OrderId,
                Status = "Reported",
                ReportedTime = currentTime,
                ManagerId = report.ManagerId,
                Notes = report.Notes
            };

            await _pickupReportRepo.AddReportAsync(newReport);
            return true;
        }

        public async Task<bool> CancelReportAsync(string reportId)
        {
            var report = await _pickupReportRepo.GetReportById(reportId);
            if (report == null) return false;

            // Check if the report was made within 20 minutes
            var reportTime = DateTime.UtcNow - report.ReportedTime;
            if (reportTime.TotalMinutes <= 20)
            {
                report.Status = "Cancelled";
                await _pickupReportRepo.UpdateReportAsync(report);
                return true;
            }

            return false; 
        }

        public async Task<bool> ApprovePickupAsync(string reportId)
        {
            var report = await _pickupReportRepo.GetReportById(reportId);
            if (report == null) return false;

            var reportTime = DateTime.UtcNow - report.ReportedTime;
            if (reportTime.TotalMinutes <= 30)
            {
                report.Status = "Approved";
                report.ApprovedTime = DateTime.UtcNow;
                await _pickupReportRepo.UpdateReportAsync(report);
                return true;
            }

            return false; 
        }
    }
}

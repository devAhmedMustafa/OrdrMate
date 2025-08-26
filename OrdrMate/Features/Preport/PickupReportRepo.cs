using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OrdrMate.Data;
using OrdrMate.DTOs;
using OrdrMate.Models;
using OrdrMate.Repositories;
public class PickupReportRepo : IPickupReportRepo
{
    private readonly OrdrMateDbContext _context;
    private readonly IMapper _mapper;

    public PickupReportRepo(OrdrMateDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task AddReportAsync(PickupReport report)
    {
        report.ReportedTime = DateTime.UtcNow;
        _context.PickupReports.Add(report);
        await _context.SaveChangesAsync();
    }

    public async Task<PickupReport> GetReportById(string reportId)
    {
        return await _context.PickupReports.FirstOrDefaultAsync(r => r.Id == reportId);
    }

    public async Task UpdateReportAsync(PickupReport report)
    {
        var existing = await _context.PickupReports.FindAsync(report.Id);
        if (existing == null) return;

        existing.Status = report.Status;
        existing.Notes = report.Notes;
        existing.ManagerId = report.ManagerId;
        _context.PickupReports.Update(existing);
        await _context.SaveChangesAsync();
    }
public async Task<bool> ApprovePickupAsync(string reportId)
        {
            var report = await _context.PickupReports.FirstOrDefaultAsync(r => r.Id == reportId);
            if (report == null) return false;

            report.Status = "Approved";
            await _context.SaveChangesAsync();

            return true;
        }
    public async Task<bool> ReportPickupAsync(PickupReportDto reportDto)
    {
        var report = _mapper.Map<PickupReport>(reportDto);
        report.Id = Guid.NewGuid().ToString();
        report.ReportedTime = DateTime.UtcNow;

        _context.PickupReports.Add(report);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CancelReportAsync(string reportId)
    {
        var existing = await _context.PickupReports.FindAsync(reportId);
        if (existing == null) return false;

        existing.Status = "Cancelled";
        await _context.SaveChangesAsync();
        return true;
    }
}

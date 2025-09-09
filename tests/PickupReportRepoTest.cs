using Microsoft.EntityFrameworkCore;
using OrdrMate.Data;
using OrdrMate.Features.Preport;
using OrdrMate.Repositories;

public class PickupReportRepoTests
{
    private readonly OrderRepo _OrderRepo;
    private readonly OrdrMateDbContext _context;

    public PickupReportRepoTests()
    {
        var options = new DbContextOptionsBuilder<OrdrMateDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;
        _context = new OrdrMateDbContext(options);
        _OrderRepo = new OrderRepo(_context);
    }
    [Fact]
    public async Task AddReportAsync_ShouldAddReport()
    {
        
        var repo = new PickupReportRepo(_context, null);
        var report = new PickupReport
        {
            Status = "Pending",
            Notes = "Test notes",
            ManagerId = "manager123",
            Id = "123",
        };

        await repo.AddReportAsync(report);
        var addedReport = await _context.PickupReports.FirstOrDefaultAsync(r => r.Id == report.Id);

        Assert.NotNull(addedReport);
        Assert.Equal("Pending", addedReport.Status);
        Assert.Equal("Test notes", addedReport.Notes);
        Assert.Equal("manager123", addedReport.ManagerId);
    }
}
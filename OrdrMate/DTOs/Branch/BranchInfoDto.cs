namespace OrdrMate.DTOs.Branch;

public class BranchInfoDto
{
    public required string BranchId { get; set; }
    public required string BranchAddress { get; set; }
    public required string BranchPhoneNumber { get; set; }
    public required string RestaurantId { get; set; }
    public required string RestaurantName { get; set; }
    public required decimal MinWaitingTime { get; set; }
    public required decimal MaxWaitingTime { get; set; }
    public required decimal AverageWaitingTime { get; set; }
    public required int FreeTables { get; set; }
    public required int TotalTables { get; set; }
    public required int OrdersInQueue { get; set; }
}
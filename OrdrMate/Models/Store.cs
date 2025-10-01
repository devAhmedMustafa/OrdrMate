namespace OrdrMate.Models;

public class Store
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public required string Name { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public required string ManagerId { get; set; }
    public User? Manager { get; set; }
    public StoreProfile? Profile { get; set; }
    public List<Branch>? Branches { get; set; } = [];
    public List<BranchRequest>? BranchRequests { get; set; }
    public List<Item>? Items { get; set; } = [];
}

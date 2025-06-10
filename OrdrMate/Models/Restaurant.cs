namespace OrdrMate.Models;

public class Restaurant
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public required string Name { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
    public required string ManagerId { get; set; }
    public User? Manager { get; set; }
    public RestaurantProfile? Profile { get; set; }
    public List<Category>? Categories { get; set; } = [];
    public List<Branch>? Branches { get; set; } = [];
    public List<BranchRequest>? BranchRequests { get; set; }
    public List<Item>? Items { get; set; } = [];
    public List<Kitchen>? Kitchens { get; set; } = [];
}

namespace OrdrMate.Models;

public class Restaurant
{
    public string Id {get; set;} = Guid.NewGuid().ToString();
    public string Name {get; set;}
    public string Phone {get; set;}
    public string Email {get; set;}
    public string ManagerId {get; set;}
}
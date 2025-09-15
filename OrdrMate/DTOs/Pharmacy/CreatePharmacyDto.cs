
namespace OrdrMate.DTOs.Pharmacy;

public class CreatePharmacyDto {
    public required string Name {get; set;}
    public required string Phone{get; set;}
    public required string Email{get; set;}
    public required string ManagerUsername{get; set;}
}
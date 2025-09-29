using OrdrMate.Models;

namespace OrdrMate.Repositories;

public interface IPharmacyRepo
{
    Task<Pharmacy> CreatePharmacy(Pharmacy pharmacy);
    Task<Pharmacy?> GetPharmacyById(string id);
    Task<bool> HasAccessToPharmacy(string managerId, string pharmacyId);

    Task<Pharmacy?> GetPharmacyByManagerId(string managerId);
    Task<IEnumerable<Pharmacy>> GetAllPharmacies();
    Task<IEnumerable<string>> GetPharmacyCategories(string pharmacyId);
    Task<IEnumerable<string>> GetPharmacyMainCategories(string pharmacyId);
    Task<PharmacyProfile?> GetPharmacyProfile(string pharmacyId);
    Task<PharmacyProfile?> UpdatePharmacyProfile(string pharmacyId, PharmacyProfile profile);
}
using Microsoft.EntityFrameworkCore;
using OrdrMate.Data;
using OrdrMate.Models;

namespace OrdrMate.Repositories;

public class PharmacyRepo(OrdrMateDbContext c) : IPharmacyRepo
{

    private readonly OrdrMateDbContext _db = c;
    public async Task<Pharmacy> CreatePharmacy(Pharmacy Pharmacy)
    {
        _db.Pharmacy.Add(Pharmacy);

        var profile = new PharmacyProfile
        {
            PharmacyId = Pharmacy.Id,
            Description = "Welcome to our Pharmacy!",
            LogoUrl = "https://example.com/default-logo.png",
            CoverImageUrl = "https://example.com/default-cover.png"
        };

        _db.PharmacyProfile.Add(profile);

        await _db.SaveChangesAsync();
        return Pharmacy;
    }

    public async Task<Pharmacy?> GetPharmacyById(string id)
    {
        return await _db.Pharmacy.Include(r => r.Profile).FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<bool> HasAccessToPharmacy(string managerId, string PharmacyId)
    {
        var Pharmacy = await _db.Pharmacy
            .FirstOrDefaultAsync(r => r.Id == PharmacyId);
        if (Pharmacy == null)
        {
            return false;
        }

        return Pharmacy.ManagerId == managerId;
    }

    public async Task<Pharmacy?> GetPharmacyByManagerId(string managerId)
    {
        return await _db.Pharmacy
            .FirstOrDefaultAsync(r => r.ManagerId == managerId);
    }

    public async Task<IEnumerable<Pharmacy>> GetAllPharmacys()
    {
        return await _db.Pharmacy.Include(r => r.Profile).ToListAsync();
    }

    public async Task<IEnumerable<Category>> GetPharmacyCategories(string PharmacyId)
    {
        var Pharmacy = await _db.Pharmacy
            .Include(r => r.Categories)
            .FirstOrDefaultAsync(r => r.Id == PharmacyId);
        if (Pharmacy == null)
        {
            throw new InvalidOperationException($"Pharmacy with id {PharmacyId} does not exist.");
        }

        return Pharmacy.Categories.Select(c => new Category
        {
            Name = c.Name,
            PharmacyId = c.PharmacyId
        }).ToList();
    }

    public async Task<PharmacyProfile?> GetPharmacyProfile(string PharmacyId)
    {
        var profile = await _db.PharmacyProfile
            .FirstOrDefaultAsync(p => p.PharmacyId == PharmacyId);

        if (profile == null)
        {
            profile = new PharmacyProfile
            {
                PharmacyId = PharmacyId,
                Description = "Welcome to our Pharmacy!",
                LogoUrl = "https://example.com/default-logo.png",
                CoverImageUrl = "https://example.com/default-cover.png"
            };
            _db.PharmacyProfile.Add(profile);
            await _db.SaveChangesAsync();
        }

        return profile;
    }

    public async Task<PharmacyProfile?> UpdatePharmacyProfile(string PharmacyId, PharmacyProfile profile)
    {
        var existingProfile = await _db.PharmacyProfile
            .FirstOrDefaultAsync(p => p.PharmacyId == PharmacyId);

        if (existingProfile == null)
        {
            throw new InvalidOperationException($"Profile for Pharmacy {PharmacyId} does not exist.");
        }

        existingProfile.Description = profile.Description;
        existingProfile.LogoUrl = profile.LogoUrl;
        existingProfile.CoverImageUrl = profile.CoverImageUrl;

        _db.PharmacyProfile.Update(existingProfile);
        await _db.SaveChangesAsync();

        return existingProfile;
    }
}
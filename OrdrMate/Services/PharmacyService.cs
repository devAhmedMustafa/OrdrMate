using OrdrMate.DTOs.Pharmacy;
using OrdrMate.Models;
using OrdrMate.Repositories;

namespace OrdrMate.Services;

public class PharmacyService(IPharmacyRepo r, IUserRepo m)
{
    private readonly IPharmacyRepo _repo = r;
    private readonly IUserRepo _managerRepo = m;

    public async Task<PharmacyDTO> CreatePharmacy(CreatePharmacyDto dto)
    {
        try
        {
            var manager = await _managerRepo.GetUserByUsername(dto.ManagerUsername);

            if (manager == null)
            {
                throw new Exception("No manager with " + dto.ManagerUsername + " username");
            }

            var Pharmacy = new Pharmacy
            {
                Name = dto.Name,
                Email = dto.Email,
                Phone = dto.Phone,
                ManagerId = manager.Id
            };

            var createdPharmacy = await _repo.CreatePharmacy(Pharmacy);

            var responseDto = new PharmacyDTO
            {
                Id = createdPharmacy.Id,
                Name = createdPharmacy.Name,
                Email = createdPharmacy.Email,
                Phone = createdPharmacy.Phone
            };

            return responseDto;
        }
        catch (Exception e)
        {
            throw new Exception($"Error creating Pharmacy: {e.Message}");
        }
    }

    public async Task<PharmacyDTO> GetPharmacyByManagerId(string id)
    {
        try
        {

            var Pharmacy = await _repo.GetPharmacyByManagerId(id);

            if (Pharmacy == null)
            {
                throw new Exception("No Pharmacy with " + id + " id");
            }

            var responseDto = new PharmacyDTO
            {
                Id = Pharmacy.Id,
                Name = Pharmacy.Name,
                Email = Pharmacy.Email,
                Phone = Pharmacy.Phone
            };

            return responseDto;
        }
        catch (Exception e)
        {
            throw new Exception($"Error getting Pharmacy: {e.Message}");
        }
    }

    public async Task<PharmacyDTO> GetPharmacyById(string id)
    {
        try
        {
            var Pharmacy = await _repo.GetPharmacyById(id);

            if (Pharmacy == null)
            {
                throw new Exception("No Pharmacy with " + id + " id");
            }

            var responseDto = new PharmacyDTO
            {
                Id = Pharmacy.Id,
                Name = Pharmacy.Name,
                Email = Pharmacy.Email,
                Phone = Pharmacy.Phone
            };

            return responseDto;
        }
        catch (Exception e)
        {
            throw new Exception($"Error getting Pharmacy: {e.Message}");
        }
    }

    public async Task<List<PharmacyDTO>> GetAllPharmacys()
    {
        try
        {
            var pharmacies = await _repo.GetAllPharmacies();
            var responseDtos = new List<PharmacyDTO>();
            foreach (var pharmacy in pharmacies)
            {
                var responseDto = new PharmacyDTO
                {
                    Id = pharmacy.Id,
                    Name = pharmacy.Name,
                    Email = pharmacy.Email,
                    Phone = pharmacy.Phone,
                    LogoUrl = pharmacy.Profile?.LogoUrl ?? string.Empty,
                    Description = pharmacy.Profile?.Description ?? string.Empty,
                    CoverUrl = pharmacy.Profile?.CoverImageUrl ?? string.Empty
                };
                
                responseDtos.Add(responseDto);
            }
            return responseDtos;
        }
        catch (Exception e)
        {
            throw new Exception($"Error getting all Pharmacys: {e.Message}");
        }
    }

    public async Task<List<CategoryDto>> GetPharmacyCategories(string pharmacyId)
    {
        try
        {
            var categories = await _repo.GetPharmacyCategories(pharmacyId);
            return [.. categories.Select(c => new CategoryDto
            {
                Name = c.Name,
            })];
        }
        catch (Exception e)
        {
            throw new Exception($"Error getting Pharmacy categories: {e.Message}");
        }
    }

    public async Task<PharmacyProfileDto> GetPharmacyProfile(string PharmacyId)
    {
        try
        {
            var profile = await _repo.GetPharmacyProfile(PharmacyId);
            if (profile == null)
            {
                throw new Exception("No profile for Pharmacy with " + PharmacyId + " id");
            }

            return new PharmacyProfileDto
            {
                PharmacyId = profile.PharmacyId,
                Description = profile.Description,
                LogoUrl = profile.LogoUrl,
                CoverImageUrl = profile.CoverImageUrl
            };
        }
        catch (Exception e)
        {
            throw new Exception($"Error getting Pharmacy profile: {e.Message}");
        }
    }

    public async Task<PharmacyProfileDto> UpdatePharmacyProfile(string id, UpdatePharmacyProfileDto profileDto)
    {
        try
        {
            var profile = await _repo.GetPharmacyProfile(id);
            if (profile == null)
            {
                throw new Exception("No profile for Pharmacy with " + id + " id");
            }

            if (profileDto.Description != null) profile.Description = profileDto.Description;
            if (profileDto.LogoUrl != null) profile.LogoUrl = profileDto.LogoUrl;
            if (profileDto.CoverImageUrl != null) profile.CoverImageUrl = profileDto.CoverImageUrl;

            var updatedProfile = await _repo.UpdatePharmacyProfile(id, profile);

            if (updatedProfile == null)
            {
                throw new Exception("No profile found for Pharmacy with " + id + " id");
            }

            return new PharmacyProfileDto
            {
                PharmacyId = updatedProfile.PharmacyId,
                Description = updatedProfile.Description,
                LogoUrl = updatedProfile.LogoUrl,
                CoverImageUrl = updatedProfile.CoverImageUrl
            };
        }
        catch (Exception e)
        {
            throw new Exception($"Error updating Pharmacy profile: {e.Message}");
        }
    }

}
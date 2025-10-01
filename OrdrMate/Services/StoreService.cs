using OrdrMate.DTOs.Store;
using OrdrMate.Models;
using OrdrMate.Repositories;

namespace OrdrMate.Services;

public class StoreService(IStoreRepo r, IUserRepo m)
{
    private readonly IStoreRepo _repo = r;
    private readonly IUserRepo _managerRepo = m;

    public async Task<StoreDTO> CreateStore(CreateStoreDto dto)
    {
        try
        {
            var manager = await _managerRepo.GetUserByUsername(dto.ManagerUsername);

            if (manager == null)
            {
                throw new Exception("No manager with " + dto.ManagerUsername + " username");
            }

            var Store = new Store
            {
                Name = dto.Name,
                Email = dto.Email,
                Phone = dto.Phone,
                ManagerId = manager.Id
            };

            var createdStore = await _repo.CreateStore(Store);

            var responseDto = new StoreDTO
            {
                Id = createdStore.Id,
                Name = createdStore.Name,
                Email = createdStore.Email,
                Phone = createdStore.Phone
            };

            return responseDto;
        }
        catch (Exception e)
        {
            throw new Exception($"Error creating Store: {e.Message}");
        }
    }

    public async Task<StoreDTO> GetStoreByManagerId(string id)
    {
        try
        {

            var Store = await _repo.GetStoreByManagerId(id);

            if (Store == null)
            {
                throw new Exception("No Store with " + id + " id");
            }

            var responseDto = new StoreDTO
            {
                Id = Store.Id,
                Name = Store.Name,
                Email = Store.Email,
                Phone = Store.Phone
            };

            return responseDto;
        }
        catch (Exception e)
        {
            throw new Exception($"Error getting Store: {e.Message}");
        }
    }

    public async Task<StoreDTO> GetStoreById(string id)
    {
        try
        {
            var Store = await _repo.GetStoreById(id);

            if (Store == null)
            {
                throw new Exception("No Store with " + id + " id");
            }

            var responseDto = new StoreDTO
            {
                Id = Store.Id,
                Name = Store.Name,
                Email = Store.Email,
                Phone = Store.Phone
            };

            return responseDto;
        }
        catch (Exception e)
        {
            throw new Exception($"Error getting Store: {e.Message}");
        }
    }

    public async Task<List<StoreDTO>> GetAllStores()
    {
        try
        {
            var stores = await _repo.GetAllStores();
            var responseDtos = new List<StoreDTO>();
            foreach (var store in stores)
            {
                var responseDto = new StoreDTO
                {
                    Id = store.Id,
                    Name = store.Name,
                    Email = store.Email,
                    Phone = store.Phone,
                    LogoUrl = store.Profile?.LogoUrl ?? string.Empty,
                    Description = store.Profile?.Description ?? string.Empty,
                    CoverUrl = store.Profile?.CoverImageUrl ?? string.Empty
                };

                responseDtos.Add(responseDto);
            }
            return responseDtos;
        }
        catch (Exception e)
        {
            throw new Exception($"Error getting all Stores: {e.Message}");
        }
    }

    public async Task<List<CategoryDto>> GetStoreCategories(string storeId)
    {
        try
        {
            var categories = await _repo.GetStoreCategories(storeId);
            return [.. categories.Select(c => new CategoryDto
            {
                Name = c,
            })];
        }
        catch (Exception e)
        {
            throw new Exception($"Error getting Store categories: {e.Message}");
        }
    }

    public async Task<StoreProfileDto> GetStoreProfile(string StoreId)
    {
        try
        {
            var profile = await _repo.GetStoreProfile(StoreId);
            if (profile == null)
            {
                throw new Exception("No profile for Store with " + StoreId + " id");
            }

            return new StoreProfileDto
            {
                StoreId = profile.StoreId,
                Description = profile.Description,
                LogoUrl = profile.LogoUrl,
                CoverImageUrl = profile.CoverImageUrl
            };
        }
        catch (Exception e)
        {
            throw new Exception($"Error getting Store profile: {e.Message}");
        }
    }

    public async Task<StoreProfileDto> UpdateStoreProfile(string id, UpdateStoreProfileDto profileDto)
    {
        try
        {
            var profile = await _repo.GetStoreProfile(id);
            if (profile == null)
            {
                throw new Exception("No profile for Store with " + id + " id");
            }

            if (profileDto.Description != null) profile.Description = profileDto.Description;
            if (profileDto.LogoUrl != null) profile.LogoUrl = profileDto.LogoUrl;
            if (profileDto.CoverImageUrl != null) profile.CoverImageUrl = profileDto.CoverImageUrl;

            var updatedProfile = await _repo.UpdateStoreProfile(id, profile);

            if (updatedProfile == null)
            {
                throw new Exception("No profile found for Store with " + id + " id");
            }

            return new StoreProfileDto
            {
                StoreId = updatedProfile.StoreId,
                Description = updatedProfile.Description,
                LogoUrl = updatedProfile.LogoUrl,
                CoverImageUrl = updatedProfile.CoverImageUrl
            };
        }
        catch (Exception e)
        {
            throw new Exception($"Error updating Store profile: {e.Message}");
        }
    }

    public async Task<List<CategoryDto>> GetStoreMainCategories(string storeId)
    {
        try
        {
            var categories = await _repo.GetStoreMainCategories(storeId);
            return [.. categories.Select(c => new CategoryDto
            {
                Name = c,
            })];
        }
        catch (Exception e)
        {
            throw new Exception($"Error getting Store main categories: {e.Message}");
        }
    }
}
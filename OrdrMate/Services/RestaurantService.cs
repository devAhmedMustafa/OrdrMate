using OrdrMate.DTOs.Pharmacy;
using OrdrMate.Models;
using OrdrMate.Repositories;

namespace OrdrMate.Services;

public class RestaurantService(IPharmacyRepo r, IUserRepo m)
{
    private readonly IPharmacyRepo _repo = r;
    private readonly IUserRepo _managerRepo = m;

    public async Task<PharmacyDTO> CreateRestaurant(CreateRestaurantDto dto)
    {
        try
        {

            var manager = await _managerRepo.GetUserByUsername(dto.ManagerUsername);

            if (manager == null)
            {
                throw new Exception("No manager with " + dto.ManagerUsername + " username");
            }

            var restaurant = new Pharmacy
            {
                Name = dto.Name,
                Email = dto.Email,
                Phone = dto.Phone,
                ManagerId = manager.Id
            };

            var createdRestaurant = await _repo.CreateRestaurant(restaurant);

            var responseDto = new PharmacyDTO
            {
                Id = createdRestaurant.Id,
                Name = createdRestaurant.Name,
            };

            if (createdRestaurant.Email != null) responseDto.Email = createdRestaurant.Email;
            if (createdRestaurant.Phone != null) responseDto.Phone = createdRestaurant.Phone;

            return responseDto;

        }
        catch (Exception e)
        {
            throw new Exception($"Error creating restaurant: {e.Message}");
        }
    }

    public async Task<PharmacyDTO> GetRestaurantByManagerId(string id)
    {
        try
        {

            var restaurant = await _repo.GetRestaurantByManagerId(id);

            if (restaurant == null)
            {
                throw new Exception("No restaurant with " + id + " id");
            }

            var responseDto = new PharmacyDTO
            {
                Id = restaurant.Id,
                Name = restaurant.Name,
            };

            if (restaurant.Email != null) responseDto.Email = restaurant.Email;
            if (restaurant.Phone != null) responseDto.Phone = restaurant.Phone;

            return responseDto;
        }
        catch (Exception e)
        {
            throw new Exception($"Error getting restaurant: {e.Message}");
        }
    }

    public async Task<PharmacyDTO> GetRestaurantById(string id)
    {
        try
        {
            var restaurant = await _repo.GetRestaurantById(id);

            if (restaurant == null)
            {
                throw new Exception("No restaurant with " + id + " id");
            }

            var responseDto = new PharmacyDTO
            {
                Id = restaurant.Id,
                Name = restaurant.Name,
            };

            if (restaurant.Email != null) responseDto.Email = restaurant.Email;
            if (restaurant.Phone != null) responseDto.Phone = restaurant.Phone;

            return responseDto;
        }
        catch (Exception e)
        {
            throw new Exception($"Error getting restaurant: {e.Message}");
        }
    }

    public async Task<List<PharmacyDTO>> GetAllRestaurants()
    {
        try
        {
            var restaurants = await _repo.GetAllRestaurants();
            var responseDtos = new List<PharmacyDTO>();
            foreach (var restaurant in restaurants)
            {
                var responseDto = new PharmacyDTO
                {
                    Id = restaurant.Id,
                    Name = restaurant.Name,
                    LogoUrl = restaurant.Profile?.LogoUrl ?? string.Empty,
                    Description = restaurant.Profile?.Description ?? string.Empty,
                    CoverUrl = restaurant.Profile?.CoverImageUrl ?? string.Empty
                };
                
                if (restaurant.Email != null) responseDto.Email = restaurant.Email;
                if (restaurant.Phone != null) responseDto.Phone = restaurant.Phone;
                responseDtos.Add(responseDto);
            }
            return responseDtos;
        }
        catch (Exception e)
        {
            throw new Exception($"Error getting all restaurants: {e.Message}");
        }
    }

    public async Task<List<CategoryDto>> GetRestaurantCategories(string restaurantId)
    {
        try
        {
            var categories = await _repo.GetRestaurantCategories(restaurantId);
            return [.. categories.Select(c => new CategoryDto
            {
                Name = c.Name,
            })];
        }
        catch (Exception e)
        {
            throw new Exception($"Error getting restaurant categories: {e.Message}");
        }
    }

    public async Task<PharmacyProfileDto> GetRestaurantProfile(string restaurantId)
    {
        try
        {
            var profile = await _repo.GetRestaurantProfile(restaurantId);
            if (profile == null)
            {
                throw new Exception("No profile for restaurant with " + restaurantId + " id");
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
            throw new Exception($"Error getting restaurant profile: {e.Message}");
        }
    }

    public async Task<PharmacyProfileDto> UpdateRestaurantProfile(string id, UpdatePharmacyProfileDto profileDto)
    {
        try
        {
            var profile = new PharmacyProfile
            {
                PharmacyId = id,
                Description = profileDto.Description,
                LogoUrl = profileDto.LogoUrl,
                CoverImageUrl = profileDto.CoverImageUrl
            };

            var updatedProfile = await _repo.UpdateRestaurantProfile(id, profile);

            if (updatedProfile == null)
            {
                throw new Exception("No profile found for restaurant with " + id + " id");
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
            throw new Exception($"Error updating restaurant profile: {e.Message}");
        }
    }

}
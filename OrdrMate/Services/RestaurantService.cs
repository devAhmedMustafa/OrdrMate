using OrdrMate.DTOs.Restaurant;
using OrdrMate.Models;
using OrdrMate.Repositories;

namespace OrdrMate.Services;

public class RestaurantService(IRestaurantRepo r, IUserRepo m)
{
    private readonly IRestaurantRepo _repo = r;
    private readonly IUserRepo _managerRepo = m;

    public async Task<RestaurantDTO> CreateRestaurant(CreateRestaurantDto dto)
    {
        try
        {

            var manager = await _managerRepo.GetUserByUsername(dto.ManagerUsername);

            if (manager == null)
            {
                throw new Exception("No manager with " + dto.ManagerUsername + " username");
            }

            var restaurant = new Restaurant
            {
                Name = dto.Name,
                Email = dto.Email,
                Phone = dto.Phone,
                ManagerId = manager.Id
            };

            var createdRestaurant = await _repo.CreateRestaurant(restaurant);

            var responseDto = new RestaurantDTO
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

    public async Task<RestaurantDTO> GetRestaurantByManagerId(string id)
    {
        try
        {

            var restaurant = await _repo.GetRestaurantByManagerId(id);

            if (restaurant == null)
            {
                throw new Exception("No restaurant with " + id + " id");
            }

            var responseDto = new RestaurantDTO
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

    public async Task<RestaurantDTO> GetRestaurantById(string id)
    {
        try
        {
            var restaurant = await _repo.GetRestaurantById(id);

            if (restaurant == null)
            {
                throw new Exception("No restaurant with " + id + " id");
            }

            var responseDto = new RestaurantDTO
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

    public async Task<List<RestaurantDTO>> GetAllRestaurants()
    {
        try
        {
            var restaurants = await _repo.GetAllRestaurants();
            var responseDtos = new List<RestaurantDTO>();
            foreach (var restaurant in restaurants)
            {
                var responseDto = new RestaurantDTO
                {
                    Id = restaurant.Id,
                    Name = restaurant.Name,
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

    public async Task<RestaurantProfileDto> GetRestaurantProfile(string restaurantId)
    {
        try
        {
            var profile = await _repo.GetRestaurantProfile(restaurantId);
            if (profile == null)
            {
                throw new Exception("No profile for restaurant with " + restaurantId + " id");
            }

            return new RestaurantProfileDto
            {
                RestaurantId = profile.RestaurantId,
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

    public async Task<RestaurantProfileDto> UpdateRestaurantProfile(string id, UpdateRestaurantProfileDto profileDto)
    {
        try
        {
            var profile = new RestaurantProfile
            {
                RestaurantId = id,
                Description = profileDto.Description,
                LogoUrl = profileDto.LogoUrl,
                CoverImageUrl = profileDto.CoverImageUrl
            };

            var updatedProfile = await _repo.UpdateRestaurantProfile(id, profile);

            if (updatedProfile == null)
            {
                throw new Exception("No profile found for restaurant with " + id + " id");
            }

            return new RestaurantProfileDto
            {
                RestaurantId = updatedProfile.RestaurantId,
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
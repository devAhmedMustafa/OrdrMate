using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrdrMate.DTOs.Store;
using OrdrMate.Services;

namespace OrdrMate.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class StoreController(StoreService r, IAuthorizationService auth) : ControllerBase
{
    private readonly StoreService _service = r;
    private readonly IAuthorizationService _authorizationService = auth;

    [HttpPost]
    public async Task<ActionResult<StoreController>> CreateStore([FromBody] CreateStoreDto dto)
    {
        try
        {
            var result = await _service.CreateStore(dto);
            return CreatedAtAction(nameof(CreateStore), new { id = result.Id }, result);
        }
        catch (Exception e)
        {
            if (e.Message.Contains("already exists"))
                return Conflict(new { err = e.Message });

            return BadRequest(new { err = e.Message });
        }
    }

    [HttpGet]
    public async Task<ActionResult<StoreDTO>> GetStoreByManagerId()
    {
        try
        {
            var managerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(managerId))
            {
                return Unauthorized(new { err = "Unauthorized" });
            }

            var Store = await _service.GetStoreByManagerId(managerId);
            if (Store == null)
            {
                return NotFound(new { err = "No Store found for this manager" });
            }

            return Ok(Store);
        }
        catch (Exception e)
        {
            return BadRequest(new { err = e.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<StoreDTO>> GetStoreById(string id)
    {
        try
        {
            var Store = await _service.GetStoreById(id);
            if (Store == null)
            {
                return NotFound(new { err = "No Store found with this ID" });
            }

            return Ok(Store);
        }
        catch (Exception e)
        {
            return BadRequest(new { err = e.Message });
        }
    }

    [HttpGet("list/all")]
    public async Task<ActionResult<List<StoreDTO>>> GetAllStores()
    {
        try
        {
            var Stores = await _service.GetAllStores();
            return Ok(Stores);
        }
        catch (Exception e)
        {
            return BadRequest(new { err = e.Message });
        }
    }

    [HttpGet("categories/{StoreId}")]
    public async Task<ActionResult<List<CategoryDto>>> GetStoreCategories(string StoreId)
    {
        try
        {
            var categories = await _service.GetStoreCategories(StoreId);
            if (categories == null || !categories.Any())
            {
                return NotFound(new { err = "No categories found for this Store" });
            }
            return Ok(categories);
        }
        catch (Exception e)
        {
            return BadRequest(new { err = e.Message });
        }
    }

    [HttpGet("main-categories/{StoreId}")]
    public async Task<ActionResult<List<CategoryDto>>> GetStoreMainCategories(string StoreId)
    {
        try
        {
            var categories = await _service.GetStoreMainCategories(StoreId);
            if (categories == null || categories.Count == 0)
            {
                return NotFound(new { err = "No main categories found for this Store" });
            }
            return Ok(categories);
        }
        catch (Exception e)
        {
            return BadRequest(new { err = e.Message });
        }
    }

    [HttpGet("profile/{StoreId}")]
    public async Task<ActionResult<StoreProfileDto>> GetStoreProfile(string StoreId)
    {
        try
        {
            var profile = await _service.GetStoreProfile(StoreId);
            if (profile == null)
            {
                return NotFound(new { err = "No profile found for this Store" });
            }
            return Ok(profile);
        }
        catch (Exception e)
        {
            return BadRequest(new { err = e.Message });
        }
    }

    [HttpPut("profile/{StoreId}")]
    public async Task<ActionResult<StoreProfileDto>> UpdateStoreProfile(string StoreId, [FromBody] UpdateStoreProfileDto profileDto)
    {
        try
        {
            var authorization = await _authorizationService.AuthorizeAsync(User, StoreId, "CanManageStore");
            if (!authorization.Succeeded)
            {
                return Forbid();
            }   

            var updatedProfile = await _service.UpdateStoreProfile(StoreId, profileDto);
            if (updatedProfile == null)
            {
                return NotFound(new { err = "No profile found for this Store" });
            }
            return Ok(updatedProfile);
        }
        catch (Exception e)
        {
            return BadRequest(new { err = e.Message });
        }
    }
}
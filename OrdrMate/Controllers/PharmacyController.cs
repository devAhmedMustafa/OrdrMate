using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrdrMate.DTOs.Pharmacy;
using OrdrMate.Services;

namespace OrdrMate.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class PharmacyController(PharmacyService r, IAuthorizationService auth) : ControllerBase
{
    private readonly PharmacyService _service = r;
    private readonly IAuthorizationService _authorizationService = auth;

    [HttpPost]
    public async Task<ActionResult<PharmacyController>> CreatePharmacy([FromBody] CreatePharmacyDto dto)
    {
        try
        {
            var result = await _service.CreatePharmacy(dto);
            return CreatedAtAction(nameof(CreatePharmacy), new { id = result.Id }, result);
        }
        catch (Exception e)
        {
            if (e.Message.Contains("already exists"))
                return Conflict(new { err = e.Message });

            return BadRequest(new { err = e.Message });
        }
    }

    [HttpGet]
    public async Task<ActionResult<PharmacyDTO>> GetPharmacyByManagerId()
    {
        try
        {
            var managerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(managerId))
            {
                return Unauthorized(new { err = "Unauthorized" });
            }

            var Pharmacy = await _service.GetPharmacyByManagerId(managerId);
            if (Pharmacy == null)
            {
                return NotFound(new { err = "No Pharmacy found for this manager" });
            }

            return Ok(Pharmacy);
        }
        catch (Exception e)
        {
            return BadRequest(new { err = e.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PharmacyDTO>> GetPharmacyById(string id)
    {
        try
        {
            var Pharmacy = await _service.GetPharmacyById(id);
            if (Pharmacy == null)
            {
                return NotFound(new { err = "No Pharmacy found with this ID" });
            }

            return Ok(Pharmacy);
        }
        catch (Exception e)
        {
            return BadRequest(new { err = e.Message });
        }
    }

    [HttpGet("list/all")]
    public async Task<ActionResult<List<PharmacyDTO>>> GetAllPharmacys()
    {
        try
        {
            var Pharmacys = await _service.GetAllPharmacys();
            return Ok(Pharmacys);
        }
        catch (Exception e)
        {
            return BadRequest(new { err = e.Message });
        }
    }

    [HttpGet("categories/{PharmacyId}")]
    public async Task<ActionResult<List<CategoryDto>>> GetPharmacyCategories(string PharmacyId)
    {
        try
        {
            var categories = await _service.GetPharmacyCategories(PharmacyId);
            if (categories == null || !categories.Any())
            {
                return NotFound(new { err = "No categories found for this Pharmacy" });
            }
            return Ok(categories);
        }
        catch (Exception e)
        {
            return BadRequest(new { err = e.Message });
        }
    }

    [HttpGet("main-categories/{PharmacyId}")]
    public async Task<ActionResult<List<CategoryDto>>> GetPharmacyMainCategories(string PharmacyId)
    {
        try
        {
            var categories = await _service.GetPharmacyMainCategories(PharmacyId);
            if (categories == null || categories.Count == 0)
            {
                return NotFound(new { err = "No main categories found for this Pharmacy" });
            }
            return Ok(categories);
        }
        catch (Exception e)
        {
            return BadRequest(new { err = e.Message });
        }
    }

    [HttpGet("profile/{PharmacyId}")]
    public async Task<ActionResult<PharmacyProfileDto>> GetPharmacyProfile(string PharmacyId)
    {
        try
        {
            var profile = await _service.GetPharmacyProfile(PharmacyId);
            if (profile == null)
            {
                return NotFound(new { err = "No profile found for this Pharmacy" });
            }
            return Ok(profile);
        }
        catch (Exception e)
        {
            return BadRequest(new { err = e.Message });
        }
    }

    [HttpPut("profile/{PharmacyId}")]
    public async Task<ActionResult<PharmacyProfileDto>> UpdatePharmacyProfile(string PharmacyId, [FromBody] UpdatePharmacyProfileDto profileDto)
    {
        try
        {
            var authorization = await _authorizationService.AuthorizeAsync(User, PharmacyId, "CanManagePharmacy");
            if (!authorization.Succeeded)
            {
                return Forbid();
            }   

            var updatedProfile = await _service.UpdatePharmacyProfile(PharmacyId, profileDto);
            if (updatedProfile == null)
            {
                return NotFound(new { err = "No profile found for this Pharmacy" });
            }
            return Ok(updatedProfile);
        }
        catch (Exception e)
        {
            return BadRequest(new { err = e.Message });
        }
    }
}
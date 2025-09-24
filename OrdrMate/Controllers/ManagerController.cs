using Microsoft.AspNetCore.Mvc;
using OrdrMate.DTOs.User;
using OrdrMate.Services;

namespace OrdrMate.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class ManagerController(ManagerService s, IConfiguration config) : ControllerBase
{

    private readonly ManagerService _service = s;
    private readonly IConfiguration _config = config;

    [HttpGet]
    public async Task<IActionResult> GetManagers()
    {
        var managers = await _service.GetAllManagers();
        return Ok(managers);
    }

    [HttpPost]
    public async Task<ActionResult<ManagerDTO>> RegisterManager([FromBody] CreateManagerDTO data)
    {

        try
        {
            var result = await _service.CreateManager(data);
            return CreatedAtAction(nameof(RegisterManager), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("already exists"))
                return Conflict(new { err = ex.Message });

            return BadRequest(new { err = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginSuccessDto>> LoginManager([FromBody] LoginDTO data)
    {
        try
        {
            var result = await _service.AuthenticateManager(data);
            return Ok(result);
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("Credentials"))
                return Unauthorized(new { err = ex.Message });

            return BadRequest(new { err = ex.Message });
        }
    }

    [HttpPost("change-role")]
    public async Task<ActionResult<ManagerDTO>> ChangeManagerRole([FromBody] ChangeRoleDTO data)
    {
        try
        {
            if (_config["Secure:AdminKey"] != Request.Headers["AdminKey"])
                return Unauthorized(new { err = "Invalid Admin Key" });

            var result = await _service.ChangeManagerRole(data.UserId, data.NewRole);
            return Ok(result);
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("not found"))
                return NotFound(new { err = ex.Message });

            return BadRequest(new { err = ex.Message });
        }
    }

}
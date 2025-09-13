using Microsoft.AspNetCore.Mvc;

namespace OrdrMate.Features.UserPassword;

[ApiController]
[Route("api/[controller]")]
public class UserPasswordController : ControllerBase
{
    private readonly UserPasswordService _userPasswordService;

    public UserPasswordController(UserPasswordService userPasswordService)
    {
        _userPasswordService = userPasswordService;
    }

    [HttpPut("change")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto data)
    {
        try
        {
            await _userPasswordService.ChangeUserPasswordAsync(data);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
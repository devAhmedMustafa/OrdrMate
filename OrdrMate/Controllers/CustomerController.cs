using Microsoft.AspNetCore.Mvc;
using OrdrMate.DTOs.User;
using OrdrMate.Services;

namespace OrdrMate.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomerController(CustomerService service, CloudMessaging cloudMessaging) : ControllerBase
{

    private readonly CustomerService _service = service ?? throw new ArgumentNullException(nameof(service));
    private readonly CloudMessaging _cloudMessaging = cloudMessaging ?? throw new ArgumentNullException(nameof(cloudMessaging));

    [HttpPost]
    public async Task<ActionResult<GoogleLoginCredentialsDto>> GoogleLogin([FromBody] GoogleLoginRequestDto dto)
    {
        if (dto == null || string.IsNullOrEmpty(dto.IdToken))
        {
            return BadRequest("Google login credentials are required.");
        }

        try
        {
            var response = await _service.AuthenticateCustomer(dto);

            if (response == null)
                return Unauthorized(new { err = "Invalid Google ID token or user not found." });

            return Ok(response);
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("Credentials"))
                return Unauthorized(new { err = ex.Message });
            else if (ex.Message.Contains("token"))
                return BadRequest(new { err = "Invalid Google ID token." });
            else
                return StatusCode(500, new { err = "Internal server error while processing Google login." });
        }

    }

    [HttpPost("firebase-token")]
    public async Task<ActionResult<string>> AssignFirebaseToken([FromBody] FirebaseTokenDto dto)
    {
        try
        {
            if (dto == null || string.IsNullOrEmpty(dto.UserId) || string.IsNullOrEmpty(dto.Token))
            {
                return BadRequest("User ID and Firebase token are required.");
            }

            var token = await _cloudMessaging.AssignTokenToUserAsync(dto.UserId, dto.Token);
            await _cloudMessaging.SendNotificationAsync(token, "Welcome to OrdrMate!", "You have successfully logged in with Google.");
            return Ok(new { token });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { err = ex.Message });
        }
    }
}
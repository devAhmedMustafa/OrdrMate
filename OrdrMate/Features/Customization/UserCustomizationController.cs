using Microsoft.AspNetCore.Mvc;

namespace OrdrMate.Features.Customization;

[ApiController]
[Route("api/[controller]")]
public class UserCustomizationController : ControllerBase
{
    private readonly UserCustomizationService _service;

    public UserCustomizationController(UserCustomizationService service)
    {
        _service = service;
    }
    
}
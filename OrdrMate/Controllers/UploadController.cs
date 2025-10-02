using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrdrMate.Features.Storage;
using OrdrMate.Features.Storage.DTOs;

namespace OrdrMate.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class UploadController(IStorageService storageService) : ControllerBase
{
    private readonly IStorageService _storageService = storageService;

    [HttpPost("presigned-url")]
    [Authorize(Roles = "TopManager")]
    public IActionResult GetUploadPresignedUrl([FromBody] UploadRequestDto request)
    {
        var result = _storageService.GetUploadPresignedUrl(request);
        
        if (!result.IsSuccess)
        {
            return StatusCode(result.StatusCode, new { error = result.ErrorMessage });
        }
        
        return Ok(result.Data);
    }

    [HttpGet("presigned-url/{filename}")]
    public IActionResult GetDownloadPresignedUrl(string filename)
    {
        var result = _storageService.GetDownloadPresignedUrl(filename);
        
        if (!result.IsSuccess)
        {
            return StatusCode(result.StatusCode, new { error = result.ErrorMessage });
        }
        
        return Ok(result.Data);
    }

    [HttpPut("upload/{fileName}")]
    public async Task<IActionResult> UploadFile(string fileName)
    {
        // Print content type and length for debugging
        Console.WriteLine($"Uploading file {Request.ContentLength} bytes");
        Console.WriteLine($"Content-Type: {Request.ContentType}");
        
        var result = await _storageService.UploadFileAsync(fileName, Request.Body, Request.ContentLength, Request.ContentType);
        
        if (!result.IsSuccess)
        {
            return StatusCode(result.StatusCode, new { error = result.ErrorMessage });
        }
        
        return Ok(result.Data);
    }
}
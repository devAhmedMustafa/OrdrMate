using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OrdrMate.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class UploadController(IWebHostEnvironment env, IConfiguration config, S3Service s3Service) : ControllerBase
{

    private readonly IWebHostEnvironment _env = env;
    private readonly S3Service _s3Service = s3Service;
    private readonly IConfiguration _config = config;

    [HttpPost("presigned-url")]
    [Authorize (Roles = "TopManager")]
    public IActionResult GetUploadPresignedUrl([FromBody] UploadRequest request)
    {
        var fileUrl = $"{Guid.NewGuid()}_{request.FileName}";
        var fileType = request.FileType;

        // if (_env.IsDevelopment())
        // {
            var uploadUrl = $"http://localhost:5126/api/Upload/upload/{fileUrl}";

            return Ok(new
            {
                uploadUrl,
                fileUrl
            });
        // }
        // if (_env.IsProduction())
        // {
        //     var bucketName = _config["AWS:BucketName"];
        //     if (string.IsNullOrEmpty(bucketName)) return StatusCode(500, "Bucket name is not configured.");
        //     var presignedUrl = _s3Service.GeneratePresignedUrl(bucketName, fileUrl, 15, Amazon.S3.HttpVerb.PUT, fileType);
        //     return Ok(new
        //     {
        //         uploadUrl = presignedUrl,
        //         fileUrl
        //     });
        // }

        // return Forbid("Not allowed in production");
    }

    [HttpGet("presigned-url/{filename}")]
    public IActionResult GetDownloadPresignedUrl(string filename)
    {
        // if (_env.IsDevelopment())
        // {
            var filePath = Path.Combine(_env.ContentRootPath, "uploads", filename);
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("File not found.");
            }
            var fileUrl = $"{_config["Host:Url"]}/uploads/{filename}";
            return Ok(new { fileUrl });
        // }
        // if (_env.IsProduction())
        // {
        //     var bucketName = _config["AWS:BucketName"];
        //     if (string.IsNullOrEmpty(bucketName)) return StatusCode(500, "Bucket name is not configured.");
        //     var presignedUrl = _s3Service.GeneratePresignedUrl(bucketName, filename, 15, Amazon.S3.HttpVerb.GET);
        //     return Ok(new { fileUrl = presignedUrl });
        // }

        // return Forbid("Not allowed in production");
    }

    [HttpPut("upload/{fileName}")]
    public async Task<IActionResult> UploadFile(string fileName)
    {
        // Print content type
        Console.WriteLine($"Uploading file {Request.ContentLength} bytes");
        Console.WriteLine($"Content-Type: {Request.ContentType}");
        if (string.IsNullOrEmpty(fileName)) return BadRequest("File name is required.");

        var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");

        if (!Directory.Exists(uploadsPath))
            Directory.CreateDirectory(uploadsPath);

        var fullPath = Path.Combine(uploadsPath, fileName);


        using var stream = new FileStream(fullPath, FileMode.Create);
        await Request.Body.CopyToAsync(stream);

        return Ok(new { filePath = fullPath });
    }

}

public class UploadRequest
{
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
}
using OrdrMate.Features.Storage.DTOs;

namespace OrdrMate.Features.Storage;

public class StorageService : IStorageService
{
    private readonly IWebHostEnvironment _env;
    private readonly S3Service _s3Service;
    private readonly IConfiguration _config;

    public StorageService(IWebHostEnvironment env, S3Service s3Service, IConfiguration config)
    {
        _env = env;
        _s3Service = s3Service;
        _config = config;
    }

    public ServiceResult<PresignedUrlResponseDto> GetUploadPresignedUrl(UploadRequestDto request)
    {
        var fileUrl = $"{Guid.NewGuid()}_{request.FileName}";
        var fileType = request.FileType;

        if (_env.IsDevelopment())
        {
            var uploadUrl = $"http://localhost:5126/api/Upload/upload/{fileUrl}";

            return ServiceResult<PresignedUrlResponseDto>.Success(new PresignedUrlResponseDto
            {
                UploadUrl = uploadUrl,
                FileUrl = fileUrl
            });
        }
        
        if (_env.IsProduction())
        {
            var bucketName = _config["AWS:BucketName"];
            if (string.IsNullOrEmpty(bucketName)) 
            {
                return ServiceResult<PresignedUrlResponseDto>.InternalError("Bucket name is not configured.");
            }
            
            var presignedUrl = _s3Service.GeneratePresignedUrl(bucketName, fileUrl, 15, Amazon.S3.HttpVerb.PUT, fileType);
            return ServiceResult<PresignedUrlResponseDto>.Success(new PresignedUrlResponseDto
            {
                UploadUrl = presignedUrl,
                FileUrl = fileUrl
            });
        }

        return ServiceResult<PresignedUrlResponseDto>.Forbidden("Not allowed in production");
    }

    public ServiceResult<DownloadUrlResponseDto> GetDownloadPresignedUrl(string filename)
    {
        if (_env.IsDevelopment())
        {
            var filePath = Path.Combine(_env.ContentRootPath, "uploads", filename);
            if (!System.IO.File.Exists(filePath))
            {
                return ServiceResult<DownloadUrlResponseDto>.NotFound("File not found.");
            }
            var fileUrl = $"http://localhost:5126/uploads/{filename}";
            return ServiceResult<DownloadUrlResponseDto>.Success(new DownloadUrlResponseDto { FileUrl = fileUrl });
        }
        
        if (_env.IsProduction())
        {
            var bucketName = _config["AWS:BucketName"];
            if (string.IsNullOrEmpty(bucketName)) 
            {
                return ServiceResult<DownloadUrlResponseDto>.InternalError("Bucket name is not configured.");
            }
            var presignedUrl = _s3Service.GeneratePresignedUrl(bucketName, filename, 15, Amazon.S3.HttpVerb.GET);
            return ServiceResult<DownloadUrlResponseDto>.Success(new DownloadUrlResponseDto { FileUrl = presignedUrl });
        }

        return ServiceResult<DownloadUrlResponseDto>.Forbidden("Not allowed in production");
    }

    public async Task<ServiceResult<UploadResultDto>> UploadFileAsync(string fileName, Stream fileStream, long? contentLength = null, string? contentType = null)
    {
        if (string.IsNullOrEmpty(fileName)) 
        {
            return ServiceResult<UploadResultDto>.Error("File name is required.");
        }

        var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");

        if (!Directory.Exists(uploadsPath))
            Directory.CreateDirectory(uploadsPath);

        var fullPath = Path.Combine(uploadsPath, fileName);

        try
        {
            using var stream = new FileStream(fullPath, FileMode.Create);
            await fileStream.CopyToAsync(stream);
            
            var fileInfo = new FileInfo(fullPath);
            
            return ServiceResult<UploadResultDto>.Success(new UploadResultDto 
            { 
                FilePath = fullPath,
                FileName = fileName,
                FileSize = fileInfo.Length
            });
        }
        catch (Exception ex)
        {
            return ServiceResult<UploadResultDto>.InternalError($"Upload failed: {ex.Message}");
        }
    }
}
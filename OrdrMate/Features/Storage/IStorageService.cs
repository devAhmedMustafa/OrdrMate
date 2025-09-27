using OrdrMate.Features.Storage.DTOs;

namespace OrdrMate.Features.Storage;

public interface IStorageService
{
    ServiceResult<PresignedUrlResponseDto> GetUploadPresignedUrl(UploadRequestDto request);
    ServiceResult<DownloadUrlResponseDto> GetDownloadPresignedUrl(string filename);
    Task<ServiceResult<UploadResultDto>> UploadFileAsync(string fileName, Stream fileStream, long? contentLength = null, string? contentType = null);
}
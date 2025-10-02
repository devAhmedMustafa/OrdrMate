namespace OrdrMate.Features.Storage.DTOs;

public class UploadRequestDto
{
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
}

public class PresignedUrlResponseDto
{
    public string UploadUrl { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
}

public class DownloadUrlResponseDto
{
    public string FileUrl { get; set; } = string.Empty;
}

public class UploadResultDto
{
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
}

public class StorageErrorDto
{
    public string ErrorMessage { get; set; } = string.Empty;
    public int StatusCode { get; set; }
}
using Microsoft.AspNetCore.Mvc;

namespace AzureBlobStorage.Controllers;

[ApiController]
[Route("[controller]")]
public class MediaController(IMediaUploader mediaUploader) : ControllerBase
{
    private readonly IMediaUploader _mediaUploader = mediaUploader;

    [HttpPost]
    [Route("single")]
    public async Task<IActionResult> UploadSingleMediaFileAsync([FromBody] UploadSingleFileRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediaUploader.UploadBlobFileAsync(request.FilePath, request.Username, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Route("multiple")]
    public async Task<IActionResult> UploadMultipleMediaFilesAsync([FromBody] UploadMultipleFilesRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediaUploader.UploadBlobFilesAsync(request.FilePaths, request.Username, cancellationToken);
        return Ok(result);
    }
}

public record UploadSingleFileRequest(string FilePath, string Username);
public record UploadMultipleFilesRequest(List<string> FilePaths, string Username);
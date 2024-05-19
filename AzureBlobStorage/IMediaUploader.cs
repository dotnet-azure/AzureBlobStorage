using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;

namespace AzureBlobStorage;

public interface IMediaUploader
{
    Task<string> UploadBlobFileAsync(string filePath, string blobContainerName, CancellationToken cancellationToken = default);
    Task<List<string>> UploadBlobFilesAsync(List<string> filePaths, string blobContainerName, CancellationToken cancellationToken = default);
}

public class MediaUploader(BlobServiceClient blobServiceClient) : IMediaUploader
{
    private readonly BlobServiceClient _blobServiceClient = blobServiceClient;

    public async Task<List<string>> UploadBlobFilesAsync(List<string> filePaths, string blobContainerName, CancellationToken cancellationToken = default)
    {
        var uploadTasks = new List<Task<string>>();

        foreach (var filePath in filePaths)
        {
            uploadTasks.Add(UploadBlobFileAsync(filePath, blobContainerName, cancellationToken));
        }

        var fileNames = await Task.WhenAll(uploadTasks);
        return fileNames.ToList();
    }

    public async Task<string> UploadBlobFileAsync(string filePath, string blobContainerName, CancellationToken cancellationToken = default)
    {
        var fileName = GenerateUniqueFileName(Path.GetExtension(filePath), Path.GetFileNameWithoutExtension(filePath));
        var containerClient = await GetBlobContainerClientAsync(blobContainerName, cancellationToken);
        var blobClient = containerClient.GetBlobClient(fileName);
        await blobClient.UploadAsync(path: filePath, overwrite: true, cancellationToken: cancellationToken);
        await blobClient.SetHttpHeadersAsync(new BlobHttpHeaders { ContentType = fileName.GetContentType() });
        return blobClient.Uri.AbsoluteUri;
    }

    private async Task<BlobContainerClient> GetBlobContainerClientAsync(string blobContainerName, CancellationToken cancellationToken)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(blobContainerName) ?? throw new Exception($"could not connect to azure blob container: {blobContainerName}");
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.BlobContainer, cancellationToken: cancellationToken);

        return containerClient;
    }

    public static string GenerateUniqueFileName(string extension, string prefix = "")
    {
        DateTime timestamp = DateTime.UtcNow;
        return $"{prefix}{timestamp:yyyyMMddHHmmssfff}{extension}";
    }
}
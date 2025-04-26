using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Components.Forms;

namespace MyAdminApp.Web.Services;

public class BlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly ILogger<BlobStorageService> _logger;
    private const string DefaultContainerName = "uploads";

    public BlobStorageService(BlobServiceClient blobServiceClient, ILogger<BlobStorageService> logger)
    {
        _blobServiceClient = blobServiceClient;
        _logger = logger;
    }

    public async Task<string?> UploadFileAsync(IBrowserFile file, string? containerName = null)
    {
        containerName ??= DefaultContainerName;

        try
        {
            // Get or create a container reference
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            // Create a unique file name
            var fileName = $"{Guid.NewGuid()}-{file.Name}";
            var blobClient = containerClient.GetBlobClient(fileName);

            // Upload the file
            using var stream = file.OpenReadStream(maxAllowedSize: 10485760); // 10MB max
            await blobClient.UploadAsync(stream, overwrite: true);

            _logger.LogInformation("File {FileName} uploaded successfully to blob storage", file.Name);
            
            // Return the URL to the blob
            return blobClient.Uri.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file {FileName} to blob storage", file.Name);
            return null;
        }
    }

    public async Task<List<BlobItemInfo>> ListFilesAsync(string? containerName = null)
    {
        containerName ??= DefaultContainerName;
        var result = new List<BlobItemInfo>();

        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            if (!await containerClient.ExistsAsync())
            {
                return result;
            }

            await foreach (var blobItem in containerClient.GetBlobsAsync())
            {
                result.Add(new BlobItemInfo
                {
                    Name = blobItem.Name,
                    Uri = $"{containerClient.Uri}/{blobItem.Name}",
                    ContentType = blobItem.Properties.ContentType,
                    Size = blobItem.Properties.ContentLength ?? 0,
                    LastModified = blobItem.Properties.LastModified?.DateTime ?? DateTime.MinValue
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing files in container {ContainerName}", containerName);
        }

        return result;
    }

    public async Task<bool> DeleteFileAsync(string fileName, string? containerName = null)
    {
        containerName ??= DefaultContainerName;

        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(fileName);

            return await blobClient.DeleteIfExistsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file {FileName} from container {ContainerName}", fileName, containerName);
            return false;
        }
    }
}

public class BlobItemInfo
{
    public string Name { get; set; } = string.Empty;
    public string Uri { get; set; } = string.Empty;
    public string? ContentType { get; set; }
    public long Size { get; set; }
    public DateTime LastModified { get; set; }
}
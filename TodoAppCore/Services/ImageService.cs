using Azure.Storage.Blobs;

namespace TodoAppCore.Services;

public class ImageService : IImageService
{
    private readonly BlobContainerClient _blobContainerClient;

    public ImageService(string containerName)
    {
        var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");

        if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));

        _blobContainerClient = new BlobContainerClient(connectionString, containerName);

        _blobContainerClient.CreateIfNotExists();
    }

    public async Task UploadAndDeleteOldVersion(string blobName, string imageData)
    {
        await _blobContainerClient.DeleteBlobIfExistsAsync(blobName);
        await _blobContainerClient.UploadBlobAsync(blobName, GetStreamFromByteArray(GetByteArrayFromString(imageData)));
    }

    public async Task<byte[]> GetImageAsByteArray(string blobName)
    {
        return await GetBytesFromStreamAsync(await GetImageAsStreamAsync(blobName));
    }

    public async Task<Stream> GetImageAsStreamAsync(string blobName)
    {
        return await _blobContainerClient.GetBlobClient(blobName).OpenReadAsync();
    }

    public byte[] GetByteArrayFromString(string imageData)
    {
        if (imageData == null) throw new ArgumentNullException(nameof(imageData));

        return Convert.FromBase64String(imageData);
    }

    public Stream GetStreamFromByteArray(byte[] imageByteArray)
    {
        if (imageByteArray == null) throw new ArgumentNullException(nameof(imageByteArray));

        return new MemoryStream(imageByteArray);
    }

    public async Task<byte[]> GetBytesFromStreamAsync(Stream stream)
    {
        if (stream == null) throw new ArgumentNullException(nameof(stream));

        var data = new byte[stream.Length];
        await stream.ReadAsync(data, 0, data.Length);
        return data;
    }
}
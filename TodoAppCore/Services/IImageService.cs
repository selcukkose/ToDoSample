namespace TodoAppCore.Services;

public interface IImageService
{
    Task UploadAndDeleteOldVersion(string blobName, string imageData);
    Task<byte[]> GetImageAsByteArray(string blobName);
    Task<Stream> GetImageAsStreamAsync(string blobName);
    Task<byte[]> GetBytesFromStreamAsync(Stream stream);
    byte[] GetByteArrayFromString(string imageData);
    Stream GetStreamFromByteArray(byte[] imageByteArray);
}
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Azure.Storage.Blobs.Specialized;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TodoAppCore.Models;
using TodoAppCore.Services;

namespace TodoAppSample.APIs;

public class ImageApi
{
    private const string TodoImageBlobContainerName = "todoimages";
    private readonly IImageService _imageService;

    public ImageApi()
    {
        _imageService = new ImageService(TodoImageBlobContainerName);
    }

    [FunctionName(nameof(ImageUpload))]
    public async Task<IActionResult> ImageUpload(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "todo/images/v1/{id}")]
        HttpRequestMessage req,
        [Blob(TodoImageBlobContainerName + "/{id}.jpg", FileAccess.ReadWrite, Connection = "AzureWebJobsStorage")]
        BlockBlobClient blobContainer,
        ILogger log)
    {
        log.LogInformation($"{nameof(ImageUpload)} triggered.");

        try
        {
            if (req.Content == null) throw new ArgumentNullException(nameof(req.Content));

            var body = await req.Content.ReadAsStringAsync();

            var imageUploadModel = JsonConvert.DeserializeObject<ImageUpload>(body);

            if (imageUploadModel.Photo == null)
                throw new ArgumentException(nameof(imageUploadModel.Photo));

            await blobContainer.UploadAsync(
                _imageService.GetStreamFromByteArray(_imageService.GetByteArrayFromString(imageUploadModel.Photo)));

            return new OkObjectResult(imageUploadModel.ToDoId);
        }
        catch (Exception e)
        {
            return new BadRequestObjectResult(e);
        }
    }

    [FunctionName(nameof(ImageUploadV2))]
    public async Task<IActionResult> ImageUploadV2(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "todo/images/v2")]
        HttpRequestMessage req,
        ILogger log)
    {
        log.LogInformation($"{nameof(ImageUploadV2)} triggered.");

        try
        {
            if (req.Content == null) throw new ArgumentNullException(nameof(req.Content));

            var body = await req.Content.ReadAsStringAsync();

            var imageUploadModel = JsonConvert.DeserializeObject<ImageUpload>(body);

            if (imageUploadModel.Photo == null || imageUploadModel.ToDoId == null)
                throw new ArgumentException(nameof(imageUploadModel.Photo) + " or " + nameof(imageUploadModel.ToDoId));

            await _imageService.UploadAndDeleteOldVersion(imageUploadModel.ToDoId, imageUploadModel.Photo);

            return new OkObjectResult(imageUploadModel.ToDoId);
        }
        catch (Exception e)
        {
            return new BadRequestObjectResult(e);
        }
    }

    [FunctionName(nameof(DownloadV2))]
    public async Task<IActionResult> DownloadV2(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todo/images/v2")]
        HttpRequest req,
        ILogger log)
    {
        log.LogInformation($"{nameof(DownloadV2)} triggered.");

        try
        {
            var blobName = req.Query["name"];

            if (string.IsNullOrEmpty(blobName)) throw new ArgumentNullException(nameof(blobName));

            var blobData = await _imageService.GetImageAsByteArray(blobName);

            return new FileContentResult(blobData, "image/jpeg");
        }
        catch (Exception e)
        {
            return new BadRequestObjectResult(e);
        }
    }
}
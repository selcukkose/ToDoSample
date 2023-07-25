using System;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using TodoAppCore.Services;

namespace TodoAppSample.Jobs;

public class ImageInfoJob
{
    private readonly ITodoService _todoService;

    public ImageInfoJob(ITodoService todoService)
    {
        _todoService = todoService;
    }

    [FunctionName("ImageInfoJob")]
    public void Run([BlobTrigger("todoimages/{id}", Connection = "AzureWebJobsStorage")] BlockBlobClient blobClient,
        string id,
        ILogger log)
    {
        try
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{id} \n");

            var todo = _todoService.GetById(RemoveFileExtension(id));

            if (todo == null) throw new ArgumentException(nameof(todo));

            todo.ImageUrl = blobClient.Uri.AbsoluteUri;
            _todoService.Update(todo);
        }
        catch (Exception e)
        {
            log.LogError(e.Message, e.Data);
            throw;
        }
    }

    private string RemoveFileExtension(string fileName)
    {
        return fileName.Replace(".jpg", string.Empty);
    }
}
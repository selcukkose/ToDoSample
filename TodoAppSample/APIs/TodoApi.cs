using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TodoAppCore.Entities;
using TodoAppCore.Services;

namespace TodoAppSample.APIs;

public class TodoApi
{
    private readonly ITodoService _todoService;

    public TodoApi(ITodoService todoService)
    {
        _todoService = todoService;
    }

    [FunctionName(nameof(Create))]
    public async Task<IActionResult> Create(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "todo")]
        HttpRequestMessage req,
        [CosmosDB(containerName: "todo-list", databaseName: "todo-database", Connection = "CosmosDbConnectionString")]
        IAsyncCollector<ToDo> todoList,
        ILogger log)
    {
        log.LogInformation($"{nameof(Create)} triggered");
        try
        {
            var todo = await ConvertRequestContentToToDo(req.Content);
            todo.Id = Guid.NewGuid().ToString();
            todo.LastUpdateDate = DateTime.UtcNow;

            await todoList.AddAsync(new ToDo
            {
                id = todo.Id,
                title = todo.Title,
                description = todo.Description,
                lastUpdateDate = todo.LastUpdateDate,
                imageUrl = todo.ImageUrl
            });

            return new OkObjectResult(todo);
        }
        catch (Exception e)
        {
            return new BadRequestObjectResult(e);
        }
    }

    [FunctionName(nameof(CreateV2))]
    public async Task<IActionResult> CreateV2(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "todo/v2")]
        HttpRequestMessage req,
        ILogger log)
    {
        log.LogInformation($"{nameof(CreateV2)} triggered");
        TodoAppCore.Models.ToDo todo;
        try
        {
            if (req.Content == null) throw new ArgumentNullException(nameof(req.Content));

            todo = await ConvertRequestContentToToDo(req.Content);

            _todoService.Add(todo);
        }
        catch (Exception e)
        {
            return new BadRequestObjectResult(e);
        }

        return new OkObjectResult(todo);
    }

    [FunctionName(nameof(Update))]
    public async Task<IActionResult> Update(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "todo/v2")]
        HttpRequestMessage req,
        ILogger log)
    {
        log.LogInformation($"{nameof(Update)} triggered");
        try
        {
            var todo = await ConvertRequestContentToToDo(req.Content);

            _todoService.Update(todo);

            return new OkObjectResult(todo);
        }
        catch (Exception e)
        {
            return new BadRequestObjectResult(e);
        }
    }

    [FunctionName(nameof(Delete))]
    public IActionResult Delete(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "todo/v2")]
        HttpRequest req,
        ILogger log)
    {
        log.LogInformation($"{nameof(Delete)} triggered");
        try
        {
            const string queryKey = "id";
            var id = req.Query[queryKey].ToString();

            if (string.IsNullOrEmpty(id)) throw new ArgumentNullException(queryKey);

            var todo = _todoService.Delete(id);

            return new OkObjectResult(todo);
        }
        catch (Exception e)
        {
            return new BadRequestObjectResult(e);
        }
    }

    [FunctionName(nameof(Get))]
    public IActionResult Get(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todo/v2")]
        HttpRequest req,
        ILogger log)
    {
        log.LogInformation($"{nameof(Get)} triggered");
        try
        {
            const string queryKey = "id";
            var id = req.Query[queryKey].ToString();

            var todoList = new List<TodoAppCore.Models.ToDo>();

            if (string.IsNullOrEmpty(id))
            {
                todoList = _todoService.GetAll();
            }
            else
            {
                var todo = _todoService.GetById(id);
                if (todo != null) todoList = new List<TodoAppCore.Models.ToDo> { todo };
            }

            return new OkObjectResult(todoList);
        }
        catch (Exception e)
        {
            return new BadRequestObjectResult(e);
        }
    }

    private async Task<TodoAppCore.Models.ToDo> ConvertRequestContentToToDo(HttpContent requestContent)
    {
        var requestAsString = await requestContent.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<TodoAppCore.Models.ToDo>(requestAsString);
    }
}
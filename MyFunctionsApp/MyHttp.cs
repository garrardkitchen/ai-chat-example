using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MyFunctionsApp;

public class MyHttp
{
    private readonly ILogger<MyHttp> _logger;

    public MyHttp(ILogger<MyHttp> logger)
    {
        _logger = logger;
    }

    [Function("MyHttp")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");
        return new OkObjectResult("Welcome to Azure Functions!");
        
    }

}
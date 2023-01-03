using System.Diagnostics;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Microsoft.AspNetCore.Mvc;
using WebApplication6.Models;

namespace WebApplication6.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IAmazonDynamoDB _dynamoDb;

    public HomeController(ILogger<HomeController> logger, IAmazonDynamoDB dynamoDb)
    {
        _logger = logger;
        _dynamoDb = dynamoDb;
    }

    public IActionResult Index()
    {
        var createTableRequest = new CreateTableRequest(
            "tableName",
            new List<KeySchemaElement>
            {
                new("pk", KeyType.HASH),
                new("sk", KeyType.RANGE)
            });

        _dynamoDb.CreateTableAsync(createTableRequest);
        
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
    }
}
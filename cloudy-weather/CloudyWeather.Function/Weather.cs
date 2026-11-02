using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CloudyWeather.Function;

public class Weather
{
    private readonly ILogger<Weather> _logger;

    public Weather(ILogger<Weather> logger)
    {
        _logger = logger;
    }

    [Function("Weather")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Weather")] HttpRequest req)
    {
        var _isLocal = Environment.GetEnvironmentVariable("SYS_ENVIRONMENT") ?? "LOCAL";
        
        if (_isLocal == "LOCAL")
        {
            return new OkObjectResult("The weather is great.");
        }
        else
        {
            return new OkObjectResult("The weather is snowy.");
        }
    }
}

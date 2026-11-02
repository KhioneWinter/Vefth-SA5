using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CloudyWeather.Function;

public class ServiceCheck
{
    private readonly ILogger<ServiceCheck> _logger;

    public ServiceCheck(ILogger<ServiceCheck> logger)
    {
        _logger = logger;
    }

    [Function("ServiceCheck")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ServiceCheck")] HttpRequest req)
    {
        var environment = Environment.GetEnvironmentVariable("SYS_ENVIRONMENT") ?? "";
        return new OkObjectResult($"The current environment is: {environment}");
    }
}

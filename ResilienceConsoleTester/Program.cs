using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using ResilienceConsoleTester;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHttpClient("dummyClient",
    httpClient => httpClient.BaseAddress = new Uri("https://localhost:7082"))
    .AddCustomPipeline();
    //.AddStandardResilienceHandler();
    //.AddStandardHedgingHandler()
    ;
var dummyClient = builder.Build().Services.GetRequiredService<IHttpClientFactory>().CreateClient("dummyClient");


foreach (var x in Enumerable.Range(1, 5))
{
    await CallDummy(x, dummyClient);
}

Console.ReadLine();

static async Task CallDummy(int callCount, HttpClient dummyClient)
{
    var stopwatch = Stopwatch.StartNew();

    try
    {
        Console.WriteLine($"{Utils.Now()}({callCount}) Starting");
        var response = await dummyClient.GetAsync("dummy");
        Console.WriteLine($"{Utils.Now()}({callCount}) Ending - {(int)response.StatusCode}: {stopwatch.Elapsed.TotalMilliseconds,10:0.00}ms");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"{Utils.Now()}({callCount}) Ending - Err: {stopwatch.Elapsed.TotalMilliseconds,10:0.00}ms - {ex.GetType().Name}");
    }
}
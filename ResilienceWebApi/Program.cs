using Polly;
using Polly.Retry;
using ResilienceWebApi.Utils;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddHttpClient();

builder.Services.AddResiliencePipeline<string, HttpResponseMessage>(Constants.Main_Retry_Pipeline,
    builder => builder
    .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
    {
        Delay = TimeSpan.FromMilliseconds(500),
        UseJitter = true,
        BackoffType = DelayBackoffType.Linear,
        MaxRetryAttempts = 2,
        OnRetry = (arg) =>
        {
            Debug.WriteLine($"We got error: {arg.Outcome.Exception?.Message ?? $"{arg.Outcome.Result?.StatusCode}, {arg.Outcome.Result?.ReasonPhrase}"}, retried: {arg.AttemptNumber}, retryDelay: {arg.RetryDelay.Microseconds} ms");
            return default;
        },
        ShouldHandle = arg => PredicateHandling(arg.Outcome)
    })
    );

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

static ValueTask<bool> PredicateHandling(Outcome<HttpResponseMessage> outcome)
{
    var pred = outcome.Exception switch
    {
        TimeoutException => true,
        InvalidOperationException => true,
        OperationCanceledException => true,
        HttpRequestException => true,
        HttpProtocolException => true,
        _ => false
    };
    var nonExpectedHttpCodes = outcome.Result?.StatusCode != System.Net.HttpStatusCode.OK;
    return new ValueTask<bool>(pred || nonExpectedHttpCodes);
}
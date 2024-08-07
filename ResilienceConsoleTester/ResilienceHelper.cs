using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using Polly.Retry;
using Polly.Telemetry;
using System.Diagnostics;

namespace ResilienceConsoleTester
{
    public static class ResilienceHelper
    {
        public static int MaxRetryAttempt = 2;
        
        public static IHttpResiliencePipelineBuilder AddCustomPipeline(this IHttpClientBuilder builder)
        {
            return builder
                .AddResilienceHandler("dummyResilienceHandler", builder => builder
                .AddConcurrencyLimiter(100)
                .AddTimeout(TimeSpan.FromSeconds(5))
                .AddFallback(new()
                {
                    ShouldHandle = arg => PredicateHandling(arg.Outcome),
                    FallbackAction = args =>
                    {
                        HttpResponseMessage fallbackResponse = ResolveFallbackResponse(args.Outcome);
                        return Outcome.FromResultAsValueTask(fallbackResponse);
                    }
                })
                //.AddHedging(new()
                //{
                //    MaxHedgedAttempts = 3,
                //    Delay = TimeSpan.FromMilliseconds(100),
                //    ShouldHandle = arg => PredicateHandling(arg.Outcome),
                //})
                //.AddRetry(new RetryStrategyOptions<HttpResponseMessage>
                //{
                //    Name = "Custom-Retry-Strategy",
                //    Delay = TimeSpan.FromMilliseconds(500),
                //    UseJitter = true,  // adds a RANDOM value between -25% and +25% of the calculated Delay
                //    BackoffType = DelayBackoffType.Linear,
                //    MaxRetryAttempts = MaxRetryAttempt,
                //    ShouldHandle = arg => PredicateHandling(arg.Outcome),
                //    OnRetry = (x) => { Console.WriteLine("attempt number: " + x.AttemptNumber); return default; }
                //})
                //.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
                //{
                //    SamplingDuration = TimeSpan.FromSeconds(5),
                //    FailureRatio = 0.5, // if 50% are failing
                //    MinimumThroughput = 3,
                //    BreakDuration = TimeSpan.FromSeconds(5),
                //})
                .AddTimeout(TimeSpan.FromSeconds(2))
                .ConfigureTelemetry(new TelemetryOptions() { TelemetryListeners = { new HttpResponseMessageTelemetryListener() }, })
                );
        }

        private static ValueTask<bool> PredicateHandling(Outcome<HttpResponseMessage> outcome)
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

        private static HttpResponseMessage ResolveFallbackResponse(Outcome<HttpResponseMessage> outcome) =>
            new(System.Net.HttpStatusCode.Conflict) { ReasonPhrase = "We really tried but... could not get a response" };
    }
}

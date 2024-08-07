using Polly;
using System.Text.Json;

// imagine we have a penson serialized and we want to turn it into an object...
string json = "{\"Name\": \"John doe\", \"Age\": 3, \"Active\": true, \"Breaks\": fals}";


var retryPolicy = new ResiliencePipelineBuilder<Persona>()
    .AddFallback(new()
    {
        ShouldHandle = args => args.Outcome switch
        {
            { Exception: JsonException } => PredicateResult.True(),
            _ => PredicateResult.False()
        },
        FallbackAction = args =>
        {
            var fallbackPersona = new Persona("Someone", 30, true, false);
            Console.WriteLine("Using default fallback persona:" + fallbackPersona + Environment.NewLine);
            return Outcome.FromResultAsValueTask(fallbackPersona);
        }
    })
    .AddRetry(new()
    {
        Delay = TimeSpan.FromSeconds(1),
        MaxDelay = TimeSpan.FromSeconds(10),
        MaxRetryAttempts = 3,
        BackoffType = DelayBackoffType.Constant,
        Name = "deserializeRetryPolicy",
        ShouldHandle = args => args.Outcome switch {
            { Exception: JsonException } => PredicateResult.True(),
            _ => PredicateResult.False()
        },
        OnRetry = (x) =>
        {
            Console.WriteLine($"Retrying action, retry attempt: {x.AttemptNumber}, retry delay: {x.RetryDelay.Seconds} seconds, " +
                $"ex: {x.Outcome.Exception?.Message}{Environment.NewLine}");
            return default;
        }
    })
    .Build();

try
{
    //var persona = JsonSerializer.Deserialize<Persona>(json);
    var persona = retryPolicy.Execute(() => JsonSerializer.Deserialize<Persona>(json));
    Console.WriteLine("we ended up with this guy:" + persona);
}
catch (Exception e)
{
    Console.WriteLine("Error, " + e.Message);
}


Console.ReadLine();

internal record struct Persona(string Name, int Age, bool Active, bool Breaks);


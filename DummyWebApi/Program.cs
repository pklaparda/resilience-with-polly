using DummyWebApi;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddExceptionHandler<MyExceptionHandler>();

var app = builder.Build();


app.UseExceptionHandler((opts) => { });
app.UseHttpsRedirection();

app.MapGet("/dummy", async (ctx) =>
{
    if (Random.Shared.NextDouble() < 0.2)
    {
        await Task.Delay(3000);
    }

    if (Random.Shared.NextDouble() < 0.5)
    {
        throw new HttpRequestException(HttpRequestError.ConnectionError, "Fail on purpose");
    }

    ctx.Response.StatusCode = 200;
    return;
});

app.Run();

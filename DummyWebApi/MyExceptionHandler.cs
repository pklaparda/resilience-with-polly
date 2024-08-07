using Microsoft.AspNetCore.Diagnostics;
using static System.Net.Mime.MediaTypeNames;

namespace DummyWebApi
{
    public class MyExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            httpContext.Response.StatusCode = 500;
            httpContext.Response.ContentType = Text.Plain;
            await httpContext.Response.WriteAsync(exception.Message);
            return true;
        }
    }
}

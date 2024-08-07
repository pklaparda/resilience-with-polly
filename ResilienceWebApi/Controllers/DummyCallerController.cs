using Microsoft.AspNetCore.Mvc;
using Polly;
using Polly.Registry;
using ResilienceWebApi.Utils;

namespace ResilienceWebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DummyCallerController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ResiliencePipeline<HttpResponseMessage> _retryPipeline;

        public DummyCallerController(IHttpClientFactory httpClientFactory, 
            ResiliencePipelineProvider<string> pipelineProvider)
        {
            _httpClientFactory = httpClientFactory;
            _retryPipeline = pipelineProvider.GetPipeline<HttpResponseMessage>(Constants.Main_Retry_Pipeline);
        }

        [HttpGet()]
        public async Task<object> Call()
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri("https://localhost:7082");

            var res = await _retryPipeline.ExecuteAsync(async cancellation => await httpClient.GetAsync("dummy", cancellation));
            Response.StatusCode = (int)res.StatusCode;
            return new { desc = res.ReasonPhrase };
        }
    }
}

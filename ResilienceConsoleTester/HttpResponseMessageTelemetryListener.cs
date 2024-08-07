using Polly.Retry;
using Polly.Telemetry;

namespace ResilienceConsoleTester
{
    public class HttpResponseMessageTelemetryListener : TelemetryListener
    {
        public override void Write<TResult, TArgs>(in TelemetryEventArguments<TResult, TArgs> args)
        {
            if (!Utils.WatchableEvents.Contains(args.Event.EventName))
                return;

            var msg = string.Empty;
            if (args.Outcome.HasValue) {
                if (args.Outcome.Value.Result is not null)
                    msg += $"StatusCode: {(int?)(args.Outcome.Value.Result as HttpResponseMessage)?.StatusCode} ";
                if (args.Outcome.Value.Exception is not null)
                    msg += $"Ex: {args.Outcome.Value.Exception?.GetType()?.Name} ";
            }

            if (args.Arguments is OnRetryArguments<HttpResponseMessage> retryArgs)
            {
                msg += $"AttemptNumber: {retryArgs.AttemptNumber}, RetryDelay: {retryArgs.RetryDelay.TotalMilliseconds}ms, IsLastRetry: {retryArgs.AttemptNumber == ResilienceHelper.MaxRetryAttempt - 1}";
            }

            Console.WriteLine($"{Utils.Now()} From Telemetry - {args.Event.Severity}, Event: '{args.Event.EventName}'. {msg}");
        }
    }
}

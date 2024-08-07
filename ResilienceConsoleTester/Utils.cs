namespace ResilienceConsoleTester
{
    public static class Utils
    {
        public static string Now() => DateTime.Now.ToString("HH:mm:ss.fff");
        public static List<string> WatchableEvents = [ 
            "ExecutionAttempt", "OnHedging", "OnRetry", "OnFallback", "OnTimeout", "OnCircuitClosed", "OnCircuitOpened", "OnCircuitHalfOpened" 
        ];
    }
}

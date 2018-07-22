namespace lib.Strategies.Features.Async
{
    public static class AwaitableExtensions
    {
        public static StrategyAwaiter GetAwaiter(this IStrategy strategy)
        {
            return new StrategyAwaiter(new[] { strategy });
        }
    }
}
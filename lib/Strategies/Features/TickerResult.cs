namespace lib.Strategies.Features
{
    public class TickerResult
    {
        public TickerResult(StrategyStatus status, IStrategy[] strategies)
        {
            Status = status;
            Strategies = strategies;
        }

        public StrategyStatus Status { get; }
        public IStrategy[] Strategies { get; }
    }
}
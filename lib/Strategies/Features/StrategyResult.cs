namespace lib.Strategies.Features
{
    public class StrategyResult
    {
        public StrategyResult(StrategyStatus status, IStrategy[] strategies)
        {
            Status = status;
            Strategies = strategies;
        }

        public StrategyStatus Status { get; }
        public IStrategy[] Strategies { get; }
    }
}
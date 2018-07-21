namespace lib.Strategies
{
    public interface IStrategy
    {
        StrategyStatus Status { get; }
        IStrategy[] Tick();
    }
}
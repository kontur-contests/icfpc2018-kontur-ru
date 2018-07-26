namespace lib.Strategies
{
    public interface IStrategy
    {
        StrategyStatus Status { get; }
        void Tick();
    }
}
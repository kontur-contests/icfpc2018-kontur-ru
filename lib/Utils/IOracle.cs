namespace lib.Utils
{
    public interface IOracle
    {
        bool TryFill(Vec cell, Vec bot);
    }
}
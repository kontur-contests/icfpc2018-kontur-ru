namespace lib.Utils
{
    public interface IOracle
    {
        bool CanFill(Vec cell, Vec bot);
        void Fill(Vec cell);
    }
}
namespace lib.Utils
{
    public class Range
    {
        public Vec Start { get; }
        public Vec End { get; }

        public Range(Vec start, Vec end)
        {
            Start = start;
            End = end;
        }

        public static Range ForShift(Vec start, Vec shift)
        {
            return new Range(start, start + shift);
        }
    }
}
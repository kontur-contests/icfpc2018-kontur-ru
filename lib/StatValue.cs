using System;
using System.Globalization;

namespace lib
{
    public class StatValue
    {
        public long Count { get; set; }
        public double Sum { get; set; }
        public double Sum2 { get; set; }
        public double Min { get; set; } = double.PositiveInfinity;
        public double Max { get; set; } = double.NegativeInfinity;

        public double StdDeviation => Math.Sqrt(Count * Sum2 - Sum * Sum) / Count;
        public double ConfIntervalSize => 2 * Math.Sqrt(Count * Sum2 - Sum * Sum) / Count / Math.Sqrt(Count);
        public double Mean => Sum / Count;


        public void Add(double value)
        {
            Count++;
            Sum += value;
            Sum2 += value * value;
            Min = Math.Min(Min, value);
            Max = Math.Max(Max, value);
        }

        public void AddAll(StatValue value)
        {
            Count += value.Count;
            Sum += value.Sum;
            Sum2 += value.Sum2;
            Min = Math.Min(Min, value.Min);
            Max = Math.Max(Max, value.Max);
        }

        public override string ToString()
        {
            return $"{Mean} +- {StdDeviation}";
        }


        public string ToDetailedString()
        {
            return $"{Mean.ToString(CultureInfo.InvariantCulture)} " +
                   $"disp={StdDeviation.ToString(CultureInfo.InvariantCulture)} " +
                   $"range={Min.ToString(CultureInfo.InvariantCulture)}..{Max.ToString(CultureInfo.InvariantCulture)} " +
                   $"confInt={ConfIntervalSize.ToString(CultureInfo.InvariantCulture)} " +
                   $"count={Count}";
        }


        public StatValue Clone()
        {
            return new StatValue
                {
                    Sum = Sum,
                    Sum2 = Sum2,
                    Max = Max,
                    Min = Min,
                    Count = Count
                };
        }
    }
}
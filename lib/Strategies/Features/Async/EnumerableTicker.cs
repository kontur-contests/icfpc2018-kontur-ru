using System;
using System.Collections.Generic;
using System.Linq;

namespace lib.Strategies.Features.Async
{
    public class EnumerableTicker
    {
        private readonly Func<IEnumerable<Result>> run;
        private readonly int attempts;
        private IEnumerator<Result> enumerator;

        public EnumerableTicker(Func<IEnumerable<Result>> run, int attempts)
        {
            this.run = run;
            this.attempts = attempts;
        }

        public StrategyStatus Tick()
        {
            if (enumerator == null)
                enumerator = run().GetEnumerator();
            for (int i = 0; i < attempts; i++)
            {
                if (enumerator.Current?.Strategies != null)
                {
                    foreach (var strategy in enumerator.Current.Strategies)
                        strategy.Tick();
                    switch (enumerator.Current.WaitType)
                    {
                    case WaitType.WaitAll:
                        enumerator.Current.Strategies.RemoveAll(s => s.Status != StrategyStatus.InProgress);
                        if (enumerator.Current.Strategies.Any())
                            return StrategyStatus.InProgress;
                        break;
                    case WaitType.WaitAny:
                        if (enumerator.Current.Strategies.All(s => s.Status == StrategyStatus.InProgress))
                            return StrategyStatus.InProgress;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                    }
                }
                if (!enumerator.MoveNext())
                {
                    enumerator.Dispose();
                    return StrategyStatus.Done;
                }
                if (enumerator.Current.Status != StrategyStatus.InProgress)
                {
                    enumerator.Dispose();
                    return enumerator.Current.Status;
                }
            }
            return StrategyStatus.InProgress;
        }

        public class Result
        {
            public WaitType WaitType { get; set; }
            public List<IStrategy> Strategies { get; set; }
            public StrategyStatus Status { get; set; }
        }
    }
}
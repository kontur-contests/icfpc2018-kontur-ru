using System;
using System.Collections.Generic;
using System.Linq;

namespace lib.Strategies.Features
{
    public class Ticker
    {
        private readonly Func<IEnumerable<TickerResult>> run;
        private IEnumerator<TickerResult> enumerator;
        private bool waiting;

        public Ticker(Func<IEnumerable<TickerResult>> run)
        {
            this.run = run;
        }

        public TickerResult Tick()
        {
            if (enumerator == null)
                enumerator = run().GetEnumerator();
            while (true)
            {
                if (!waiting)
                {
                    if (enumerator.MoveNext())
                    {
                        if (enumerator.Current.Status == StrategyStatus.Incomplete)
                            waiting = true;
                        else
                            enumerator.Dispose();
                        return enumerator.Current;
                    }
                    enumerator.Dispose();
                    return new TickerResult(StrategyStatus.Done, null);
                }

                if (enumerator.Current.Strategies.Any(s => s.Status == StrategyStatus.Incomplete))
                    return new TickerResult(StrategyStatus.Incomplete, null);

                waiting = false;
            }
        }
    }
}
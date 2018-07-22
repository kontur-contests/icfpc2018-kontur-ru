using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using lib.Commands;
using lib.Models;

namespace lib.Strategies.Features
{
    public class Ticker
    {
        private readonly Func<IEnumerable<StrategyResult>> run;
        private IEnumerator<StrategyResult> enumerator;
        private bool waiting;

        public Ticker(Func<IEnumerable<StrategyResult>> run)
        {
            this.run = run;
        }

        public StrategyResult Tick()
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
                    return new StrategyResult(StrategyStatus.Done, null);
                }

                if (enumerator.Current.Strategies.Any(s => s.Status == StrategyStatus.Incomplete))
                    return new StrategyResult(StrategyStatus.Incomplete, null);

                waiting = false;
            }
        }
    }
}
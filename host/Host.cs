using System;
using System.Diagnostics;
using System.Threading;

using log4net;

namespace host
{
    public class Host
    {
        private readonly ManualResetEventSlim stopSignal;
        private readonly ILog log;
        private readonly TimeSpan tickInterval = TimeSpan.FromMilliseconds(1000);

        public Host(ManualResetEventSlim stopSignal, ILog log)
        {
            this.stopSignal = stopSignal;
            this.log = log;
        }

        public void Run()
        {
            do
            {
                var sw = Stopwatch.StartNew();
                try
                {
                    HandleTick();
                }
                catch (Exception exception)
                {
                    log.Error("HandleTick failed", exception);
                }
                finally
                {
                    sw.Stop();
                    if (sw.Elapsed > tickInterval)
                        log.Error($"HandleTick is too slow for tickInterval: {tickInterval}, sw.Elapsed: {sw.Elapsed}");
                }
            } while (!stopSignal.Wait(tickInterval));
        }

        private void HandleTick()
        {
            log.Info("Tick");
        }
    }
}
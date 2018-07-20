using System;
using System.Diagnostics;

using Metrics;

namespace lib
{
    public static class Metrics
    {
        static Metrics()
        {
            var processName = Process.GetCurrentProcess().ProcessName.Replace('.', '_');
            var hostName = Environment.MachineName.Replace('.', '_');
            Metric.SetGlobalContextName($"ICFPC18.{processName}.{hostName}");
            var graphiteUri = new Uri("net.tcp://graphite-relay.skbkontur.ru:2003");
            Metric.Config.WithReporting(x => x.WithGraphite(graphiteUri, TimeSpan.FromMinutes(1)));
        }

        public static MetricsContext Root { get; } = Metric.Context("KonturRu");
    }
}
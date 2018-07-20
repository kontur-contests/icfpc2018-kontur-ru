using System;
using System.Threading;

using Kontur.Houston.Plugin.Local;
using Kontur.Logging;

using log4net.ElasticSearch.Filters;

namespace houston
{
    public class HoustonRunnerEntryPoint
    {
        public static void Main(string[] args)
        {
            FilterByHardCodedEnvironmentFilter.CurrentEnvironment.Value = "production";

            var cts = new CancellationTokenSource();

            var daemonTask = PluginRunner.Create(new HoustonRunner())
                                         .WithProperties(new HoustonRunnerProperties())
                                         .WithInfo()
                                         .WithLog(new ColorConsoleLog())
                                         .StartAsync(cts.Token);

            Console.ReadKey();

            cts.Cancel();
            daemonTask.GetAwaiter().GetResult();

            Console.WriteLine("Press any key...");
            Console.ReadKey();
        }
    }
}
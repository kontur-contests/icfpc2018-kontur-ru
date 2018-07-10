using System;
using System.Threading;
using System.Threading.Tasks;

using lib;

namespace host
{
    public static class HostEntryPoint
    {
        private static readonly ManualResetEventSlim stopSignal = new ManualResetEventSlim();

        public static void Main()
        {
            var log = Log.For("HostLog");
            AppDomain.CurrentDomain.UnhandledException += (_, eventArgs) =>
                {
                    log.Fatal("Unhandled exception in current AppDomain", (Exception)eventArgs.ExceptionObject);
                    Environment.ExitCode = 1;
                };
            TaskScheduler.UnobservedTaskException += (_, eventArgs) =>
                {
                    log.Fatal("UnobservedTaskException", eventArgs.Exception);
                    eventArgs.SetObserved();
                };
            Console.CancelKeyPress += (_, eventArgs) =>
                {
                    log.Info("Ctrl+C is pressed -> terminating...");
                    stopSignal.Set();
                    eventArgs.Cancel = true;
                };
            try
            {
                log.Info("Host started");
                new Host(stopSignal, log).Run();
                log.Info("Host stopped");
            }
            catch (Exception e)
            {
                log.Fatal("Unhandled exception on the main thread", e);
                Environment.ExitCode = 3;
            }
        }
    }
}
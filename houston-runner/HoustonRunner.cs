using System;
using System.Threading;

using Kontur.Houston.Plugin;

using lib;

using Nest;

namespace houston
{
    public class HoustonRunner : IPlugin<HoustonRunnerProperties>
    {
        public void Run(IPluginContext<HoustonRunnerProperties> context)
        {
            var client = new ElasticClient(new ConnectionSettings(new Uri("http://efk2-elasticsearch9200.efk2.10.217.14.7.xip.io")).DefaultIndex("test2"));

            while (!context.CancellationToken.IsCancellationRequested)
            {
                // using houston-provided properties
                context.Log.Info(context.Properties.message);

                Log.For(this).Info("Hello via log4stash");

                Thread.Sleep(10000);

                // using elastic response
                var response = client.Get<TestMessage>("1");
                if (response.Source != null)
                    context.Log.Info(response.Source.Text);
                else
                {
                    context.Log.Info("Failed to fetch message from Elastic");
                    context.Log.Info(response.DebugInformation);
                }

                Thread.Sleep(10000);
            }

            context.Log.Info("Bye!");
        }
    }
}
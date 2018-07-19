using System;
using System.Threading;

using Kontur.Houston.Plugin;

using Nest;

public class HoustonRunner : IPlugin<HoustonRunnerProperties>
{
    class Message
    {
        public string Text;
    }
    
    public void Run(IPluginContext<HoustonRunnerProperties> context)
    {
        var settings = new ConnectionSettings(new Uri("http://efk2-elasticsearch9200.efk2.10.217.14.7.xip.io"))
            .DefaultIndex("test");

        var client = new ElasticClient(settings);
        
        while (!context.CancellationToken.IsCancellationRequested)
        {
            // using houston-provided properties
            context.Log.Info(context.Properties.message);

            Thread.Sleep(10000);
           
            // using elastic response
            var response = client.Get<Message>("1");
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
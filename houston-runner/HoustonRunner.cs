using System.Threading;

using Kontur.Houston.Plugin;

public class HoustonRunner : IPlugin<HoustonRunnerProperties>
{
    public void Run(IPluginContext<HoustonRunnerProperties> context)
    {
        while (!context.CancellationToken.IsCancellationRequested)
        {
            context.Log.Info(context.Properties.message);
 
            Thread.Sleep(1000);
        }
 
        context.Log.Info("Bye!");
    }
}
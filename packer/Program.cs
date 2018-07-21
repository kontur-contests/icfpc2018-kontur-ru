using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using lib;
using lib.ElasticDTO;
using lib.Utils;

using Nest;

namespace packer
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new ElasticClient(new ConnectionSettings(new Uri("http://efk2-elasticsearch9200.efk2.10.217.14.7.xip.io")).DefaultIndex("testruns"));

            var searchResponse = client.Search<TaskRunMeta>(
                s => s.RequestConfiguration(r => r.DisableDirectStreaming())
                      .Query(q => q.Term(p => p.Field(f => f.IsSuccess).Value(true)))
                      .Size(0)
                      .Aggregations(aggs => aggs.Terms("task_name",
                                                       terms => terms.Field("taskName.keyword")
                                                                     .Size(1000)
                                                                     .Aggregations(childAggs => childAggs
                                                                                                    .Min("min_energy", min => min.Field("energySpent"))))));
            Console.Out.WriteLine(FileHelper.SolutionsDir);
            if (Directory.Exists(FileHelper.SolutionsDir))
                Directory.Delete(FileHelper.SolutionsDir, true);
            Directory.CreateDirectory(FileHelper.SolutionsDir);
            foreach (var bucket in searchResponse.Aggregations.Terms("task_name").Buckets)
            {
                var taskName = bucket.Key;
                var energySpent = bucket.Min("min_energy").Value;

                var docSearchResponse = client.Search<TaskRunMeta>(s => s
                                                                            .RequestConfiguration(r => r.DisableDirectStreaming())
                                                                            .Query(q => q.Bool(b => b.Should(bs => bs.Term(p => p.Field(f => f.TaskName).Value(taskName)),
                                                                                                             bs => bs.Term(p => p.Field(f => f.EnergySpent).Value(energySpent))))));
                foreach (var document in docSearchResponse.Documents)
                {
                    var solutionBase64 = document.Solution;
                    var solutionContent = solutionBase64.SerializeSolutionFromString();
                    var targetSolutionPath = Path.Combine(FileHelper.SolutionsDir, $"{document.TaskName.Split('_')[0]}.nbt");
                    var infoMessage = $"Save solution for task '{document.TaskName}' with energy '{document.EnergySpent}' to the '{targetSolutionPath}'";
                    Console.Error.WriteLine(infoMessage);
                    Log.For("packer").Info(infoMessage);
                    File.WriteAllBytes(targetSolutionPath, solutionContent);
                }
            }
        }
    }
}
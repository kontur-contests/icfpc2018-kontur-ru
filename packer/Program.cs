using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using lib.ElasticDTO;

using Nest;

namespace packer
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new ElasticClient(new ConnectionSettings(new Uri("http://efk2-elasticsearch9200.efk2.10.217.14.7.xip.io")).DefaultIndex("testruns"));

            var searchResponse = client.Search<TaskRunMeta>(s => s
                .RequestConfiguration(r => r.DisableDirectStreaming())
                .Query(q => q.Term(p => p.Field(f => f.IsSuccess).Value(true)))                                                 
                .Size(0)                                            
                .Aggregations(aggs => aggs.Terms("task_name", terms => terms
                    .Field("taskName.keyword")
                    .Size(1000)
                    .Aggregations(childAggs => childAggs
                        .Min("min_energy", min => min.Field("energySpent"))
                    ))));

            foreach (var bucket in searchResponse.Aggregations.Terms("task_name").Buckets)
            {
                var taskName = bucket.Key;
                var energySpent = bucket.Min("min_energy").Value;

                var docSearchResponse = client.Search<TaskRunMeta>(s => s
                    .RequestConfiguration(r => r.DisableDirectStreaming())
                    .Query(q => q.Bool(b => b.Should(bs => bs.Term(p => p.Field(f => f.TaskName).Value(taskName)),
                                                     bs => bs.Term(p => p.Field(f => f.EnergySpent).Value(energySpent))))));
//                Console.WriteLine(docSearchResponse.DebugInformation);
            }
        }
    }
}

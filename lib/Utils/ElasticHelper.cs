using System;
using System.Collections.Generic;
using System.Linq;

using lib.ElasticDTO;

using MoreLinq;

using Nest;

namespace lib.Utils
{
    public static class ElasticHelper
    {
        private const string elasticUrl = "http://efk2-elasticsearch9200.efk2.10.217.14.7.xip.io";
        private const string elasticIndex = "testruns";
        
        public static HashSet<string> FetchSolvedProblemNames()
        {
            var client = new ElasticClient(
                new ConnectionSettings(new Uri(elasticUrl))
                    .DefaultIndex(elasticIndex)
                    .DisableDirectStreaming()
                );
            
            var aggResponse = client.Search<TaskRunMeta>(
                s => s.Query(q => q.Term(p => p.Field(f => f.IsSuccess).Value(true)))
                      .Size(0)
                      .Aggregations(
                          aggs => aggs.Terms(
                              "task_name",
                              terms => terms.Field("taskName.keyword")
                                            .Size(100000))));

            if (!aggResponse.IsValid)
                throw new Exception($"Could not fetch solved problem names: {aggResponse.DebugInformation}");
                    
            return aggResponse.Aggregations.Terms("task_name").Buckets.Select(b => b.Key).ToHashSet();
        }
    }
}
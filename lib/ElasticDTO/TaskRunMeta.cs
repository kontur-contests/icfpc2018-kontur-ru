using System;

using Nest;

using Newtonsoft.Json;

namespace lib.ElasticDTO
{
    public class TaskRunMeta
    {
        public DateTime StartedAt;
        public int SecondsSpent;
        public long EnergySpent;
        public string TaskName;
        public string SolverName;
        public string RunningHostName;
        public bool IsSuccess;

        [Binary(Store = false)]
        public string Solution { get; set; }
    }
}
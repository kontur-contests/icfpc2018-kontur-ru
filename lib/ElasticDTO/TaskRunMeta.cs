using System;
using System.Collections.Generic;

using Nest;

using Newtonsoft.Json;

namespace lib.ElasticDTO
{
    public class TaskRunMeta
    {
        public DateTime StartedAt;
        public int SecondsSpent;
        public long EnergySpent;
        public List<long> EnergyHistory;
        public string TaskName;
        public string SolverName;
        public string RunningHostName;
        public bool IsSuccess;
        public string ExceptionInfo;

        [Binary(Store = false)]
        public string Solution { get; set; }
    }
}
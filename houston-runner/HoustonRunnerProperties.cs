using System;

namespace houston
{
    public class HoustonRunnerProperties
    {
        public TimeSpan SolverStartTimeout = TimeSpan.FromMinutes(1);
        public TimeSpan SolverTimeoutMeasureInterval = TimeSpan.FromMinutes(1);
        public TimeSpan SolverTimeout = TimeSpan.FromMinutes(10);
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using JetBrains.Annotations;

using lib;
using lib.Commands;
using lib.Models;
using lib.Strategies;
using lib.Strategies.Features;
using lib.Utils;

using MoreLinq;

using NUnit.Framework;

namespace tests
{
    [TestFixture]
    // ReSharper disable once InconsistentNaming
    public class SolveAndPostResults_USE_ME_TO_POST_SOLUTIONS_FROM_YOUR_COMPUTER
    {
        [Test]
        [Explicit]
        public void SolveAndPost([Values("FD003")] string problemName)
        {
            var problem = ProblemSolutionFactory.LoadProblem(problemName);
        }
    }
}
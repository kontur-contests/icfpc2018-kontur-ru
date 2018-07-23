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
        private readonly ResultsPoster poster = new ResultsPoster();
        private readonly Evaluator evaluator = new Evaluator();

        [Test]
        [Explicit]
        public void AssembleAndPost([Values(11)] int problemId)
        {
            var problem = ProblemSolutionFactory.LoadProblem($"FA{problemId:D3}");
            var solution = new Solution
                {
                    Name = "special",
                    Solver = p =>
                        {
                            var state = new DeluxeState(p.SourceMatrix, p.TargetMatrix);
                            return new Solver(state, new AssembleOneBox(state));
                        }
                };
            Evaluate(problem, solution, postToElastic: true);
        }

        [Test]
        [Explicit]
        public void DisassembleAndPost([Values(4)] int problemId)
        {
            var problem = ProblemSolutionFactory.LoadProblem($"FD{problemId:D3}");
            //var solution = ProblemSolutionFactory.blockDeconstructor;
            var solution = ProblemSolutionFactory.CreateInvertingDisassembler(ProblemSolutionFactory.CreateSlicerAssembler(6, 6));
            Evaluate(problem, solution, postToElastic: true);
        }

        [Test]
        [Explicit]
        public void ReassembleAndPost([Values(4)] int problemId)
        {
            var problem = ProblemSolutionFactory.LoadProblem($"FR{problemId:D3}");
            //var solution = ProblemSolutionFactory.CreateReassembler(ProblemSolutionFactory.blockDeconstructor, ProblemSolutionFactory.CreateSlicerAssembler(6, 6));
            var solution = ProblemSolutionFactory.CreateReassembler(
                ProblemSolutionFactory.CreateInvertingDisassembler(ProblemSolutionFactory.CreateSlicerAssembler(6, 6)), 
                ProblemSolutionFactory.CreateSlicerAssembler(6, 6));
            Evaluate(problem, solution, postToElastic:true);
        }

        private void Evaluate(Problem problem, Solution solution, bool postToElastic = true)
        {
            var result = evaluator.Run(problem, solution);
            Console.WriteLine(result);
            if (postToElastic)
                poster.PostResult(result);
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using lib;
using lib.Commands;
using lib.Models;
using lib.Strategies;
using lib.Utils;

using NUnit.Framework;

namespace tests
{
    [TestFixture]
    public class SolveAllTest
    {
        [Test]
        public void Solve()
        {
            var problemsDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "../../../../data/problemsL");
            var resultsDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "../../../../data/results");
            var allProblems = Directory.EnumerateFiles(problemsDir, "*.mdl");
            Parallel.ForEach(allProblems, p =>
                {
                    var mtrx = Matrix.Load(File.ReadAllBytes(p));
                    var problem = mtrx.Clone();
                    var R = mtrx.N;
                    var solver = new GreedyPartialSolver(mtrx.Voxels, new bool[R, R, R], new Vec(0, 0, 0), new ThrowableHelper(mtrx));
                    try
                    {
                        solver.Solve();
                    }
                    catch (Exception e)
                    {
                        Log.For(this).Error($"Unhandled exception in solver for {Path.GetFileName(p)}", e);
                        return;
                    }
                    var commands = solver.Commands.ToArray();

                    // todo (andrew, 21.07.2018): Валидация работает некорректно :-(
                    /*if (!TryValidate(problem, commands, p))
                        return;*/

                    var bytes = CommandSerializer.Save(commands);
                    File.WriteAllBytes(Path.Combine(resultsDir, $"{Path.GetFileNameWithoutExtension(p)}.nbt"), bytes);
                });
        }

        private bool TryValidate(Matrix problem, ICommand[] commands, string p)
        {
            try
            {
                Validate(problem, commands);
            }
            catch (Exception e)
            {
                Log.For(this).Error($"Invalid solution for {Path.GetFileName(p)}", e);
                return false;
            }
            return true;
        }

        private void Validate(Matrix problem, ICommand[] solution)
        {
            var state = new MutableState(problem);
            var queue = new Queue<ICommand>(solution);
            while (queue.Any())
                state.Tick(queue);
            state.EnsureIsFinal();
        }
    }
}
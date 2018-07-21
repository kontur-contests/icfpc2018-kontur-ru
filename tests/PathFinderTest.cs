using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using FluentAssertions;

using lib.Models;
using lib.Strategies;
using lib.Utils;

using NUnit.Framework;

namespace tests
{
    [TestFixture]
    public class PathFinderTest
    {
        [Test]
        public void METHOD()
        {
            var R = 5;
            var state = new bool[R, R, R];
            for (int x = 0; x < R; x++)
            for (int y = 0; y < R; y++)
            for (int z = 0; z < R; z++)
            {
                state.Set(new Vec(x,y,z), true);
            }

            var start = new Vec(0, 0, 0);
            var shifts = new[]
                {
                    new Vec(1, 0, 0), new Vec(1, 0, 0), new Vec(1, 0, 0),
                    new Vec(0, 1, 0), new Vec(0, 1, 0), new Vec(0, 1, 0),
                    new Vec(0, 0, 1), new Vec(0, 0, 1), new Vec(0, 0, 1),
                    new Vec(-1, 0, 0), new Vec(-1, 0, 0), new Vec(-1, 0, 0),
                };
            var current = start;
            state.Set(current, false);
            foreach (var shift in shifts)
            {
                current += shift;
                state.Set(current, false);
            }

            var solver = new PathFinder(state, new Vec(0, 0, 0), new Vec(0, 3, 3));
            var path = solver.TryFindPath();
            foreach (var command in path)
            {
                Console.Out.WriteLine(command);
            }
        }

        [Test]
        public void Test()
        {
            var hashes = new HashSet<int>();
            for (int x = 0; x < 250; x++)
            for (int y = 0; y < 250; y++)
            for (int z = 0; z < 250; z++)
            {
                hashes.Add(new Vec(x, y, z).GetHashCode());
            }
            hashes.Should().HaveCount(250 * 250 * 250);
        }

        [Test]
        public void Solve()
        {
            var problemsDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "../../../../data/problemsL");
            var p = Directory.EnumerateFiles(problemsDir, "*.mdl").Single(x => x.Contains("007"));
            var mtrx = Matrix.Load(File.ReadAllBytes(p));
            var R = mtrx.N;
            var solver = new GreedyPartialSolver(mtrx.Voxels, new bool[R, R, R], new Vec(0, 0, 0), new ThrowableHelper(mtrx));
            solver.Solve();
        }
    }
}
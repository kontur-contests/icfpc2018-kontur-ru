using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using lib.Commands;
using lib.Models;
using lib.Utils;

using Microsoft.AspNetCore.Mvc;

namespace ui.Controllers
{
    [Route("api/[controller]")]
    public class MatrixController : Controller
    {
        [HttpGet("[action]")]
        public string[,,] Index(int i)
        {
            const string problemsDir = "../data/problemsL";
            var filename = Directory.EnumerateFiles(problemsDir, "*.mdl").ToList()[i];

            if (filename == null) return null;

            var content = System.IO.File.ReadAllBytes(filename);

            var voxels = Matrix.Load(content).Voxels;
            var strings = new string[voxels.GetLength(0), voxels.GetLength(1), voxels.GetLength(2)];

            for (var x = 0; x < voxels.GetLength(0); x++)
                for (var y = 0; y < voxels.GetLength(1); y++)
                    for (var z = 0; z < voxels.GetLength(2); z++)
                        strings[x, y, z] = voxels[x, y, z] ? "" : "0";

            return strings;
        }

        [HttpGet("[action]")]
        public IEnumerable<string> Solutions()
        {
            return Directory
                .EnumerateFiles("../data/solutions/")
                .Select(Path.GetFileNameWithoutExtension)
                .OrderBy(x => x);
        }

        [HttpGet("[action]")]
        public TraceResult Trace(string file, int startTick = 0, int count = 2000)
        {
            var problemName = file.Split("-")[0];
            var problem = Matrix.Load(System.IO.File.ReadAllBytes($"../data/problemsL/{problemName}_tgt.mdl"));
            var solution = CommandSerializer.Load(System.IO.File.ReadAllBytes($"../data/solutions/{file}.nbt"));

            var state = new MutableState(new Matrix(problem.R));
            var queue = new Queue<ICommand>(solution);

            var results = new List<TickResult>();
            var filledVoxels = new HashSet<Vec>();

            try
            {
                var newFilledVoxels = new List<Vec>();
                var tickIndex = 0;
                while (queue.Any() && tickIndex < startTick + count)
                {
                    state.Tick(queue);


                    foreach (var vec in state.LastChangedCells)
                        if (state.BuildingMatrix[vec] && !filledVoxels.Contains(vec))
                        {
                            newFilledVoxels.Add(vec);
                            filledVoxels.Add(vec);
                        }
                    if (tickIndex >= startTick)
                    {
                        results.Add(new TickResult
                            {
                                changes = newFilledVoxels.Select(v => new[] {v.X, v.Y, v.Z}).ToArray(),
                                bots = state.Bots
                                            .Select(x => new[] {x.Position.X, x.Position.Y, x.Position.Z})
                                            .ToArray(),
                                energy = state.Energy
                            });
                        newFilledVoxels.Clear();
                    }
                    tickIndex++;
                }
            }
            catch (Exception)
            {
                // Ignore failed simulation
            }

            var ticks = results.ToArray();
            return new TraceResult
                {
                    R = problem.R,
                    startTick = startTick,
                    totalTicks = results.Count,
                    Ticks = ticks
                };
        }
    }

    public struct TraceResult
    {
        public int startTick;
        public int totalTicks;
        public int R;
        public TickResult[] Ticks;
    }

    public struct TickResult
    {
        public int[][] changes;
        public int[][] bots;
        public long energy;
    }
}
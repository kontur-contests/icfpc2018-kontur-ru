using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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
            {
                for (var y = 0; y < voxels.GetLength(1); y++)
                {
                    for (var z = 0; z < voxels.GetLength(2); z++)
                    {
                        strings[x, y, z] = voxels[x, y, z] ? "" : "0";
                    }
                }
            }

            return strings;
        }

        [HttpGet("[action]")]
        public ArrayList Check007()
        {
            var problem = Matrix.Load(System.IO.File.ReadAllBytes("../data/problemsL/LA007_tgt.mdl"));
            var solution = CommandSerializer.Load(System.IO.File.ReadAllBytes("LA007.nbt"));
            
            var state = new MutableState(new Matrix(problem.R));
            var queue = new Queue<ICommand>(solution);

            var results = new ArrayList();
            var filledVoxels = new ArrayList();

            try
            {
                while (queue.Any())
                {
                    state.Tick(queue);

                    var newFilledVoxels = new ArrayList();
                    
                    for (var x = 0; x < state.Matrix.R; ++x)
                    for (var y = 0; y < state.Matrix.R; ++y)
                    for (var z = 0; z < state.Matrix.R; ++z)
                    {
                        var vec = new Vec(x, y, z);
                        if (state.Matrix[x, y, z] && !filledVoxels.Contains(vec))
                        {
                            newFilledVoxels.Add(vec);
                            filledVoxels.Add(vec);
                        }
                    }

                    results.Add(new TickResult
                        {
                            change = newFilledVoxels,
                            bots = state.Bots
                                .Select(x => Tuple.Create(x.Position.X, x.Position.Y, x.Position.Z))
                                .ToArray()
                        });
                }
            }
            catch (Exception)
            {
                // Ignore failed simulation
            }

            return results;
        }

        public struct TickState
        {
            public Matrix matrix;
            public IEnumerable bots;
        }
    }

    public struct TickResult
    {
        public ArrayList change;
        public Tuple<int, int, int>[] bots;
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
        public IEnumerable<string> Solutions()
        {
            return Directory
                .EnumerateFiles("../data/solutions/")
                .Select(Path.GetFileNameWithoutExtension)
                .OrderBy(x => x);
        }

        [HttpGet("[action]")]
        public ArrayList Trace(string file)
        {
            var problem = Matrix.Load(System.IO.File.ReadAllBytes($"../data/problemsL/{file}_tgt.mdl"));
            var solution = CommandSerializer.Load(System.IO.File.ReadAllBytes($"../data/solutions/{file}.nbt"));
            
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
                    
                    for (var x = 0; x < state.BuildingMatrix.R; ++x)
                    for (var y = 0; y < state.BuildingMatrix.R; ++y)
                    for (var z = 0; z < state.BuildingMatrix.R; ++z)
                    {
                        var vec = new Vec(x, y, z);
                        if (state.BuildingMatrix[x, y, z] && !filledVoxels.Contains(vec))
                        {
                            newFilledVoxels.Add(vec);
                            filledVoxels.Add(vec);
                        }
                    }

                    results.Add(new TickResult
                        {
                            R = state.BuildingMatrix.R,
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
    }

    public struct TickResult
    {
        public int R;
        public ArrayList change;
        public Tuple<int, int, int>[] bots;
    }
}
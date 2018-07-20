using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using lib.Models;

using Microsoft.AspNetCore.Mvc;

namespace ui.Controllers
{
    [Route("api/[controller]")]
    public class MatrixController : Controller
    {
        [HttpGet("[action]")]
        public int[,,] Index(int i)
        {
            const string problemsDir = "../data/problemsL";
            var filename = Directory.EnumerateFiles(problemsDir, "*.mdl").ToList()[i];

            if (filename == null) return null;
            
            var content = System.IO.File.ReadAllBytes(filename);

            var voxels = Matrix.Load(content).Voxels;
            var ints = new int[voxels.GetLength(0),voxels.GetLength(1),voxels.GetLength(2)];

            for (var x = 0; x < voxels.GetLength(0); x++)
            {
                for (var y = 0; x < voxels.GetLength(1); x++)
                {
                    for (var z = 0; x < voxels.GetLength(2); x++)
                    {
                        ints[x, y, z] = voxels[x, y, z] ? 1 : 0;
                    }
                }
            }

            return ints;
        }
    }
}
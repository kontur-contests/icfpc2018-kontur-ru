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
        public bool[,,] Index(int i)
        {
            const string problemsDir = "../data/problemsL";
            var filename = Directory.EnumerateFiles(problemsDir, "*.mdl").ToList()[i];

            if (filename == null) return null;
            
            var content = System.IO.File.ReadAllBytes(filename);
            return Matrix.Load(content).Voxels;
        }
    }
}
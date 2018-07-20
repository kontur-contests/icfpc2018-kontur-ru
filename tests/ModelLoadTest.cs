using System;
using System.Collections.Generic;
using System.IO;

using lib.Models;

using NUnit.Framework;

namespace tests
{
    [TestFixture]
    public class ModelLoadTest
    {
        [Test]
        public void SaveModel()
        {
            var model = new Matrix("01|00", "10|11");
            Assert.IsTrue(model[1, 0, 0]);
            Assert.IsTrue(model[0, 0, 1]);
            Assert.IsTrue(model[0, 1, 1]);
            Assert.IsTrue(model[1, 1, 1]);
            Assert.AreEqual(new byte[]{2, 0x9a}, model.Save());
        }

        [Test]
        [Explicit]
        public void LoadSample([ValueSource(nameof(GetModels))]string filename)
        {
            Console.WriteLine(filename);
            var content = File.ReadAllBytes(filename);
            var model = Matrix.Load(content);
            var newContent = model.Save();
            Assert.AreEqual(newContent, content);
        }

        private static IEnumerable<string> GetModels()
        {
            var problemsDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "../../../../data/problemsL");
            foreach (string file in Directory.EnumerateFiles(problemsDir, "*.mdl"))
            {
                yield return file;
            }
        }
    }
}
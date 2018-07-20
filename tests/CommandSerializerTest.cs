using System.Collections.Generic;
using System.IO;

using lib.Utils;

using NUnit.Framework;

namespace tests
{
    [TestFixture]
    public class CommandSerializerTest
    {
        [Test]
        [Explicit]
        public void TestOnSamleTraces([ValueSource(nameof(GetModels))]string filename)
        {
            var content = File.ReadAllBytes(filename);
            var commands = CommandSerializer.Load(content);
            var newContent = CommandSerializer.Save(commands);
            Assert.AreEqual(content, newContent);
        }

        private static IEnumerable<string> GetModels()
        {
            var problemsDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "../../../../data/dfltTracesL");
            foreach (string file in Directory.EnumerateFiles(problemsDir, "*.nbt"))
            {
                yield return file;
            }
        }
    }
}

using System.Collections.Generic;
using System.IO;
using System.Linq;

using lib.Commands;
using lib.Models;
using lib.Utils;

using NUnit.Framework;

namespace tests
{
    [TestFixture]
    public class CommandApplyingTest
    {
        [TestCaseSource(nameof(GetModels))]
        public void TestOnSamleTraces(string commandFilename, string problemFilename)
        {
            var content = File.ReadAllBytes(commandFilename);
            var commands = CommandSerializer.Load(content);
            var state = new MutableState(Matrix.Load(File.ReadAllBytes(problemFilename)));
            var queue = new Queue<ICommand>(commands);
            while (queue.Any())
                state.Tick(queue);
            state.EnsureIsFinal();
        }

        private static IEnumerable<object[]> GetModels()
        {
            var problemsDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "../../../../data");
            foreach (var (commands, model) in Directory.EnumerateFiles(problemsDir, "dfltTracesF/*.nbt").Zip(Directory.EnumerateFiles(problemsDir, "problemsF/*.mdl"), (a, b) => (a, b)))
            {
                yield return new [] {commands, model};
            }
        }
    }
}

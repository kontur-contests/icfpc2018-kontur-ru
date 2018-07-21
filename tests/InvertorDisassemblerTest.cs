using System;
using System.Collections.Generic;
using System.Linq;

using lib.Commands;
using lib.Primitives;
using lib.Strategies;
using lib.Utils;

using MoreLinq;

using NUnit.Framework;

namespace tests
{
    [TestFixture]
    public class InvertorDisassemblerTest
    {
        [Test]
        public void Test()
        {
            var commands = new Queue<ICommand>(new ICommand[]
                {
                    new Fission(new NearDifference(new Vec(1, 0, 0)), 1),

                    new Wait(),
                    new Fission(new NearDifference(new Vec(0, 1, 0)), 0),

                    new Wait(),
                    new Wait(),
                    new SMove(new LongLinearDifference(new Vec(-1, 0, 0))),

                    new FusionP(new NearDifference(new Vec(0, 1, 0))),
                    new Wait(),
                    new FusionS(new NearDifference(new Vec(0, -1, 0))),

                    new FusionP(new NearDifference(new Vec(1, 0, 0))),
                    new FusionS(new NearDifference(new Vec(-1, 0, 0))),

                    new Halt()
                });
            var delimitedCommands = commands.ToDelimitedString(", ");
            Console.WriteLine(delimitedCommands);
            var reversed = InvertorDisassembler.ReverseCommands(commands, 10).ToList();
            Console.WriteLine(reversed.ToDelimitedString(", "));
            var reversedTwice = InvertorDisassembler.ReverseCommands(new Queue<ICommand>(reversed), 10).ToList();
            Console.WriteLine(reversedTwice.ToDelimitedString(", "));
            Assert.AreEqual(delimitedCommands, reversedTwice.ToDelimitedString(", "));
        }
    }
}
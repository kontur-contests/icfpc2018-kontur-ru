using JetBrains.Annotations;

using lib.Commands;
using lib.Primitives;
using lib.Utils;

using NUnit.Framework;

namespace tests
{
    [TestFixture]
    public class CommandEncodingTest
    {
        [TestCaseSource(nameof(cases))]
        public void TestOnDocumentationSamples([NotNull] ICommand command, [NotNull] byte[] expected)
        {
            Assert.AreEqual(expected, command.Encode());
        }

        private static object[] cases =
            {
                new object[] {new Halt(), new byte [] {0b11111111}},
                new object[] {new Wait(), new byte [] {0b11111110}},
                new object[] {new Flip(), new byte [] {0b11111101}},
                new object[] {new SMove(new LongLinearDifference(new Vec(12, 0, 0))), new byte [] {0b00010100, 0b00011011}},
                new object[] {new SMove(new LongLinearDifference(new Vec(0, 0, -4))), new byte [] {0b00110100, 0b00001011}},
                new object[] {new LMove(new ShortLinearDifference(new Vec(3, 0, 0)), new ShortLinearDifference(new Vec(0, -5, 0))), new byte [] {0b10011100, 0b00001000}},
                new object[] {new LMove(new ShortLinearDifference(new Vec(0, -2, 0)), new ShortLinearDifference(new Vec(0, 0, 2))), new byte [] {0b11101100, 0b01110011}},
                new object[] {new FusionP(new NearDifference(new Vec(-1, 1, 0))), new byte [] {0b00111111}},
                new object[] {new FusionS(new NearDifference(new Vec(1, -1, 0))), new byte [] {0b10011110}},
                new object[] {new Fission(new NearDifference(new Vec(0, 0, 1)), 5), new byte [] {0b01110101, 0b00000101}},
                new object[] {new Fill(new NearDifference(new Vec(0, -1, 0))), new byte [] {0b01010011}},
                new object[] {new Void(new NearDifference(new Vec(1, 0, 1))), new byte [] {0b10111010}},
                new object[] {new GFill(new NearDifference(new Vec(0, -1, 0)), new FarDifference(new Vec(10, -15, 20))), new byte[] {0b01010001, 0b00101000, 0b00001111, 0b00110010}},
                new object[] {new GVoid(new NearDifference(new Vec(1, 0, 0)), new FarDifference(new Vec(5, 5, -5))), new byte[] {0b10110000, 0b00100011, 0b00100011, 0b00011001}},
            };
    }
}

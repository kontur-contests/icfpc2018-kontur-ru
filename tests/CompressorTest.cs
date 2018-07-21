using System.Text;

using FluentAssertions;

using lib.Utils;

using NUnit.Framework;

namespace tests
{
    [TestFixture]
    public class CompressorTest
    {
        [Test]
        public void Test()
        {
            var bytes = Encoding.ASCII.GetBytes("lalala");
            var compressed = bytes.Compress();
            var decompressed = compressed.Decompress();
            var decompressedString = Encoding.ASCII.GetString(decompressed);
            decompressedString.Should().Be("lalala");
        }
    }
}
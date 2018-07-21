using FluentAssertions;

using lib.Models;
using lib.Utils;

using NUnit.Framework;

namespace tests
{
    [TestFixture]
    internal class ThrowableHelperTests
    {
        [Test]
        public void Test()
        {
            var toFill = new Matrix(3);

            var a = new Vec(0, 0, 1);
            var b = new Vec(0, 1, 0);
            var c = new Vec(1, 0, 0);

            var helper = new ThrowableHelper(toFill);

            helper.CanFill(a, b).Should().BeTrue();
            helper.Fill(a);
            helper.CanFill(b, c).Should().BeTrue();
            helper.Fill(b);

            helper.CanFill(c, new Vec(2, 2, 2)).Should().BeFalse();
            helper.CanFill(c, new Vec(0, 0, 0)).Should().BeTrue();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace lib
{
    [TestFixture]
    public class SampleTest
    {
        [Test]
        public void Test()
        {
            (2+2).Should().Be(4);
        }
    }
}

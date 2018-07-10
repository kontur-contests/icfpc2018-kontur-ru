using FluentAssertions;

using NUnit.Framework;

namespace tests
{
    [TestFixture]
    public class SampleTest
    {
        [Test]
        public void Failure()
        {
            Assert.Fail("sample fail");
        }

        [Test]
        public void Success()
        {
            Assert.That(true, Is.True);
        }

        [Test]
        public void FluentAssertion()
        {
            "The quick brown fox...".Should().StartWith("The");
        }
    }
}
using FluentAssertions;

using lib;

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

        [Test]
        public void Logging()
        {
            Log.For(this).Info("Sample test log");
        }
    }
}
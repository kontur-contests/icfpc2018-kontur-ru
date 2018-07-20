using FluentAssertions;

using lib.Models;

using NUnit.Framework;

namespace tests
{
    [TestFixture]
    public class ComponentTrackingMatrixTest
    {
        [TestCase("0")]
        [TestCase("1")]
        [TestCase("10|11", "00|00")]
        [TestCase("10|11", "00|01")]
        [TestCase("101|101|101", "010|010|010", "100|100|100")]
        public void OnlyGrounded(params string[] matrix)
        {
            var componentTrackingMatrix = new ComponentTrackingMatrix(new Matrix(matrix));
            componentTrackingMatrix.HasNonGroundedVoxels.Should().BeFalse();
        }

        [TestCase("00|10", "00|00")]
        [TestCase("00|11", "00|00")]
        [TestCase("00|01", "00|10")]
        [TestCase("10|01", "00|11")]
        [TestCase("101|101|101", "000|010|010", "101|101|101")]
        public void HasNotGrounded(params string[] matrix)
        {
            var componentTrackingMatrix = new ComponentTrackingMatrix(new Matrix(matrix));
            componentTrackingMatrix.HasNonGroundedVoxels.Should().BeTrue();
        }

        [Test]
        public void TrackComponents()
        {
            var componentTrackingMatrix = new ComponentTrackingMatrix(new Matrix("10|01", "00|00"));
            componentTrackingMatrix[0, 1, 0] = true;
            componentTrackingMatrix.HasNonGroundedVoxels.Should().BeFalse();
        }
    }
}
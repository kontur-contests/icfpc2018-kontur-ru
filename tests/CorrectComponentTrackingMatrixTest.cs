using System;

using FluentAssertions;

using lib.Models;
using lib.Utils;

using NUnit.Framework;

namespace tests
{
    [TestFixture]
    public class CorrectComponentTrackingMatrixTest
    {
        [TestCase("0")]
        [TestCase("1")]
        [TestCase("10|11", "00|00")]
        [TestCase("10|11", "00|01")]
        [TestCase("101|101|101", "010|010|010", "100|100|100")]
        public void OnlyGrounded(params string[] matrix)
        {
            var componentTrackingMatrix = new CorrectComponentTrackingMatrix(new Matrix(matrix));
            componentTrackingMatrix.HasNonGroundedVoxels.Should().BeFalse();
        }

        [TestCase("00|10", "00|00")]
        [TestCase("00|11", "00|00")]
        [TestCase("00|01", "00|10")]
        [TestCase("10|01", "00|11")]
        [TestCase("101|101|101", "000|010|010", "101|101|101")]
        public void HasNotGrounded(params string[] matrix)
        {
            var componentTrackingMatrix = new CorrectComponentTrackingMatrix(new Matrix(matrix));
            componentTrackingMatrix.HasNonGroundedVoxels.Should().BeTrue();
        }

        [Test]
        public void TrackComponents()
        {
            var componentTrackingMatrix = new CorrectComponentTrackingMatrix(new Matrix("10|01", "00|00"));
            componentTrackingMatrix.HasNonGroundedVoxels.Should().BeTrue();
            componentTrackingMatrix[0, 1, 0] = true;
            componentTrackingMatrix.HasNonGroundedVoxels.Should().BeFalse();
            componentTrackingMatrix[0, 1, 0] = false;
            componentTrackingMatrix.HasNonGroundedVoxels.Should().BeTrue();
            componentTrackingMatrix[0, 1, 0] = true;
            componentTrackingMatrix.HasNonGroundedVoxels.Should().BeFalse();
        }

        [TestCase(10, 0, 100)]
        [TestCase(20, 1, 1000000)]
        [TestCase(100, 2, 10000)]
        [TestCase(200, 3, 1000000)]
        public void TrackComponents2(int r, int seed, int iterations)
        {
            var matrix = new Matrix(r);
            var correctCTM = new CorrectComponentTrackingMatrix(matrix.Clone());
            var usualCTM = new ComponentTrackingMatrix(matrix.Clone());
            var random = new Random(seed);
            for (var i = 0; i < iterations; i++)
            {
                var position = new Vec(random.Next(r), random.Next(r), random.Next(r));
                correctCTM[position] = true;
                usualCTM[position] = true;
                correctCTM.HasNonGroundedVoxels.Should().Be(usualCTM.HasNonGroundedVoxels);
            }
        }

        [Test]
        public void TrackComponents3()
        {
            var componentTrackingMatrix = new CorrectComponentTrackingMatrix(new Matrix("000|101|000", "000|000|000", "000|000|000"));
            componentTrackingMatrix.HasNonGroundedVoxels.Should().BeTrue();
            componentTrackingMatrix[1, 1, 0] = true;
            componentTrackingMatrix.HasNonGroundedVoxels.Should().BeTrue();
            componentTrackingMatrix[1, 0, 0] = true;
            componentTrackingMatrix.HasNonGroundedVoxels.Should().BeFalse();
            componentTrackingMatrix[1, 0, 0] = false;
            componentTrackingMatrix.HasNonGroundedVoxels.Should().BeTrue();
        }
    }
}
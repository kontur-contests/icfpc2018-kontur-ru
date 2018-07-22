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
            var componentTrackingMatrix = new CorrectComponentTrackingMatrix(new Matrix(matrix).Voxels);
            componentTrackingMatrix.HasNonGroundedVoxels.Should().BeFalse();
        }

        [TestCase("00|10", "00|00")]
        [TestCase("00|11", "00|00")]
        [TestCase("00|01", "00|10")]
        [TestCase("10|01", "00|11")]
        [TestCase("101|101|101", "000|010|010", "101|101|101")]
        public void HasNotGrounded(params string[] matrix)
        {
            var componentTrackingMatrix = new CorrectComponentTrackingMatrix(new Matrix(matrix).Voxels);
            componentTrackingMatrix.HasNonGroundedVoxels.Should().BeTrue();
        }

        [Test]
        public void TrackComponents()
        {
            var componentTrackingMatrix = new CorrectComponentTrackingMatrix(new Matrix("10|01", "00|00").Voxels);
            componentTrackingMatrix.HasNonGroundedVoxels.Should().BeTrue();
            componentTrackingMatrix[0, 1, 0] = true;
            componentTrackingMatrix.HasNonGroundedVoxels.Should().BeFalse();
            componentTrackingMatrix[0, 1, 0] = false;
            componentTrackingMatrix.HasNonGroundedVoxels.Should().BeTrue();
            componentTrackingMatrix[0, 1, 0] = true;
            componentTrackingMatrix.HasNonGroundedVoxels.Should().BeFalse();
        }

        [Test]
        public void TrackComponents3()
        {
            var componentTrackingMatrix = new CorrectComponentTrackingMatrix(new Matrix("000|101|000", "000|000|000", "000|000|000").Voxels);
            componentTrackingMatrix.HasNonGroundedVoxels.Should().BeTrue();
            componentTrackingMatrix[1, 1, 0] = true;
            componentTrackingMatrix.HasNonGroundedVoxels.Should().BeTrue();
            componentTrackingMatrix[1, 0, 0] = true;
            componentTrackingMatrix.HasNonGroundedVoxels.Should().BeFalse();
            componentTrackingMatrix[1, 0, 0] = false;
            componentTrackingMatrix.HasNonGroundedVoxels.Should().BeTrue();
        }

        [Test]
        public void TrackComponents4()
        {
            var componentTrackingMatrix = new CorrectComponentTrackingMatrix(new Matrix("000|000|000", "000|000|000", "000|000|000").Voxels);
            componentTrackingMatrix.HasNonGroundedVoxels.Should().BeFalse();
            componentTrackingMatrix[0, 0, 0] = true;
            componentTrackingMatrix.HasNonGroundedVoxels.Should().BeFalse();
            componentTrackingMatrix[1, 0, 0] = true;
            componentTrackingMatrix.HasNonGroundedVoxels.Should().BeFalse();
            componentTrackingMatrix[2, 0, 0] = true;
            componentTrackingMatrix.HasNonGroundedVoxels.Should().BeFalse();
            componentTrackingMatrix[2, 0, 0] = false;
            componentTrackingMatrix.HasNonGroundedVoxels.Should().BeFalse();
        }

        [Test]
        public void TrackComponents6()
        {
            var componentTrackingMatrix = new CorrectComponentTrackingMatrix(new Matrix("000|101|001", "110|101|000", "111|011|101").Voxels);
            componentTrackingMatrix[0, 1, 2] = true;
            componentTrackingMatrix.HasNonGroundedVoxels.Should().BeFalse();
        }

        [Test]
        public void EnableGroundedCell()
        {
            var componentTrackingMatrix = new CorrectComponentTrackingMatrix(new Matrix("000|000|000", "010|000|000", "000|000|000").Voxels);
            componentTrackingMatrix[1, 0, 1] = true;
            componentTrackingMatrix.HasNonGroundedVoxels.Should().BeFalse();
        }

        [Test]
        public void DisableUngroundedCell()
        {
            var componentTrackingMatrix = new CorrectComponentTrackingMatrix(new Matrix("000|000|000", "000|010|000", "000|000|000").Voxels);
            componentTrackingMatrix[1, 1, 1] = false;
            componentTrackingMatrix.HasNonGroundedVoxels.Should().BeFalse();
        }
    }
}
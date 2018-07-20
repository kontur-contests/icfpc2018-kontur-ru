using System;
using System.IO;

using JetBrains.Annotations;

using lib.Commands;
using lib.Primitives;

namespace lib.Utils
{
    public static class CommandSerializer
    {
        public static ICommand[] Load([NotNull] byte[] content)
        {
            throw new NotImplementedException();
        }

        public static ICommand LoadSingleCommand(BinaryReader binaryReader)
        {
            var firstByte = binaryReader.ReadByte();
            if (firstByte.TryExtractMask("11111111", out var _))
                return new Halt();
            if (firstByte.TryExtractMask("11111110", out var _))
                return new Wait();
            if (firstByte.TryExtractMask("11111101", out var _))
                return new Flip();
            // todo (sivukhin, 21.07.2018): Validate parsed parameter?
            if (firstByte.TryExtractMask("*****111", out var fusionPNearDistance))
                return new FusionP(NearLinearDistance.ParseFromParameter(fusionPNearDistance));
            if (firstByte.TryExtractMask("*****110", out var fusionSNearDistance))
                return new FusionP(NearLinearDistance.ParseFromParameter(fusionSNearDistance));
            if (firstByte.TryExtractMask("*****011", out var fillNearDistance))
                return new Fill(NearLinearDistance.ParseFromParameter(fillNearDistance));
            var secondByte = binaryReader.ReadByte();
            throw new NotImplementedException();
        }
    }
}
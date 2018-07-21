using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

using JetBrains.Annotations;

using lib.Commands;
using lib.Primitives;

namespace lib.Utils
{
    public static class Compressor
    {
        [NotNull]
        public static string SerializeSolutionToString([NotNull] this byte[] solutionContent)
        {
            return Convert.ToBase64String(solutionContent.Compress());
        }

        [NotNull]
        public static byte[] SerializeSolutionFromString([NotNull] this string solutionString)
        {
            return Convert.FromBase64String(solutionString).Decompress();
        }

        public static byte[] Compress(this byte[] data)
        {
            var memoryStream = new MemoryStream();
            using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create))
            {
                var entry = zipArchive.CreateEntry("data", CompressionLevel.Optimal);
                using (var stream = entry.Open())
                    stream.Write(data, 0, data.Length);
            }
            return memoryStream.ToArray();
        }

        public static byte[] Decompress(this byte[] data)
        {
            var dataStream = new MemoryStream(data);
            using (var zipArchive = new ZipArchive(dataStream, ZipArchiveMode.Read))
            {
                var entry = zipArchive.GetEntry("data");
                using (var stream = entry.Open())
                {
                    var result = new MemoryStream();
                    stream.CopyTo(result);
                    return result.ToArray();
                }
            }
        }
    }

    public static class CommandSerializer
    {
        [NotNull]
        public static byte[] Save([NotNull] ICommand[] commands)
        {
            return commands.SelectMany(x => x.Encode()).ToArray();
        }

        [NotNull]
        public static ICommand[] Load([NotNull] byte[] content)
        {
            var commands = new List<ICommand>();
            using (var stream = new BinaryReader(new MemoryStream(content)))
            {
                while (stream.BaseStream.Position != stream.BaseStream.Length)
                    commands.Add(LoadSingleCommand(stream));
            }
            return commands.ToArray();
        }

        [NotNull]
        private static ICommand LoadSingleCommand([NotNull] BinaryReader binaryReader)
        {
            var firstByte = binaryReader.ReadByte();
            if (firstByte.TryExtractMask("11111111", out _))
                return new Halt();
            if (firstByte.TryExtractMask("11111110", out _))
                return new Wait();
            if (firstByte.TryExtractMask("11111101", out _))
                return new Flip();
            if (firstByte.TryExtractMask("*****111", out var fusionPNearDistance))
                return new FusionP(new NearDifference(fusionPNearDistance));
            if (firstByte.TryExtractMask("*****110", out var fusionSNearDistance))
                return new FusionP(new NearDifference(fusionSNearDistance));
            if (firstByte.TryExtractMask("*****011", out var fillNearDistance))
                return new Fill(new NearDifference(fillNearDistance));
            var secondByte = binaryReader.ReadByte();
            if (firstByte.TryExtractMask("00**0100", out var sMoveA) &&
                secondByte.TryExtractMask("000*****", out var sMoveI))
                return new SMove(new LongLinearDifference(sMoveA, sMoveI));
            if (firstByte.TryExtractMask("****1100", out var lMoveFirstP) &&
                secondByte.TryExtractMask("********", out var lMoveSecondP))
            {
                var shortDistance2A = lMoveFirstP >> 2;
                var shortDistance1A = lMoveFirstP & 0b11;
                var shortDistance2I = lMoveSecondP >> 4;
                var shortDistance1I = lMoveSecondP & 0b1111;
                return new LMove(new ShortLinearDifference(shortDistance1A, shortDistance1I),
                                 new ShortLinearDifference(shortDistance2A, shortDistance2I));
            }

            if (firstByte.TryExtractMask("*****101", out var fissionNearDistance))
                return new Fission(new NearDifference(fissionNearDistance), secondByte);
            throw new Exception($"Can't parse command from the stream: [{firstByte}, {secondByte}, ...]");
        }
    }
}

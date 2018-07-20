using System;

using JetBrains.Annotations;

namespace lib.Utils
{
    public static class ByteExtensions
    {
        public static bool TryExtractMask(this byte value, [NotNull] string mask, out int maskedValue)
        {
            if (mask.Length != 8)
                throw new ArgumentException("mask should has length = 8", nameof(mask));
            maskedValue = 0;
            for (int i = 0; i < 8; i++)
            {
                var actualBit = (value >> (7 - i)) & 1;
                if (mask[i] == '*')
                    maskedValue = maskedValue * 2 + actualBit;
                else
                {
                    var expectedBit = mask[i] == '1' ? 1 : 0;
                    if (expectedBit != actualBit)
                        return false;
                }
            }
            return true;
        }
    }
}
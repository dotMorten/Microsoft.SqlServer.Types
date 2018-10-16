using System;
using System.Runtime.CompilerServices;

namespace Microsoft.SqlServer.Types
{
    internal class BitPattern
    {
        internal BitPattern(long minValue, long maxValue, string pattern)
        {
            MinValue = minValue;
            MaxValue = maxValue;
            PatternOnes = GetBitMask(pattern, c => c == '1');
            PatternMask = GetBitMask(pattern, c => c == 'x');
            BitLength = (ushort)pattern.Length;

            var prefix = pattern.Substring(0, pattern.IndexOf('x'));

            PrefixOnes = GetBitMask(prefix, c => c == '1');
            PrefixBitLength = (ushort)prefix.Length;
            Pattern = pattern;
        }

        private ulong GetBitMask(string pattern, Func<char, bool> isOne)
        {
            ulong result = 0;
            foreach (char c in pattern)
                result = (result << 1) | (isOne(c) ? (ulong)1 : 0);
            return result;
        }

        internal long MinValue { get; }
        internal long MaxValue { get; }
        internal string Pattern { get; }

        internal ulong PatternOnes { get; }
        internal ulong PatternMask { get; }
        internal int BitLength { get; }

        internal ulong PrefixOnes { get; }
        internal int PrefixBitLength { get; }

        internal bool ContainsValue(int value)
        {
            return MinValue <= value && value <= MaxValue;
        }

        public override string ToString() => Pattern;

        public ulong EncodeValue(int val, bool isLast)
        {
            ulong expand = Expand(PatternMask, (int)(val - MinValue));

            ulong value = PatternOnes | expand | 1;

            if (!isLast)
                value++;

            return value;
        }

        private ulong Expand(ulong mask, int value)
        {
            if (mask == 0)
                return 0;

            if ((mask & 0x1) > 0)
                return Expand(mask >> 1, value >> 1) << 1 | ((ulong)value & 0x1);

            return Expand(mask >> 1, value) << 1;
        }

        internal int Decode(ulong encodedValue, out bool isLast)
        {
            var decodedValue = Compress(encodedValue, PatternMask);

            isLast = (encodedValue & 0x1) == 0x1;
            return (int)((isLast ? decodedValue : decodedValue - 1) + MinValue);
        }

        private long Compress(ulong value, ulong mask)
        {
            if (mask == 0)
                return 0;

            if ((mask & 0x1) > 0)
                return Compress(value >> 1, mask >> 1) << 1 | (long)(value & 0x1);

            return Compress(value >> 1, mask >> 1);
        }
    }
}

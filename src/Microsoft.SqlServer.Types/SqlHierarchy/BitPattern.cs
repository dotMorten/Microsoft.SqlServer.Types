using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Microsoft.SqlServer.Types.Tests")]

namespace Microsoft.SqlServer.Types
{
    class BitPattern
    {
        public BitPattern(long minValue, long maxValue, string pattern)
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

        ulong GetBitMask(string pattern, Func<char, bool> isOne)
        {
            ulong result = 0;
            foreach (char c in pattern)
                result = (result << 1) | (isOne(c) ? (ulong)1 : 0);
            return result;
        }

        public long MinValue { get; }
        public long MaxValue { get; }
        public string Pattern { get; }

        public ulong PatternOnes { get; }
        public ulong PatternMask { get; }
        public int BitLength { get; }

        public ulong PrefixOnes { get; }
        public int PrefixBitLength { get; }

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

        ulong Expand(ulong mask, int value)
        {
            if (mask == 0)
                return 0;

            if ((mask & 0x1) > 0)
                return Expand(mask >> 1, value >> 1) << 1 | ((ulong)value & 0x1);

            return Expand(mask >> 1, value) << 1;
        }

        public int Decode(ulong encodedValue, out bool isLast)
        {
            var decodedValue = Compress(encodedValue, PatternMask);

            isLast = (encodedValue & 0x1) == 0x1;
            return (int)((isLast ? decodedValue : decodedValue - 1) + MinValue);
        }

        long Compress(ulong value, ulong mask)
        {
            if (mask == 0)
                return 0;

            if ((mask & 0x1) > 0)
                return Compress(value >> 1, mask >> 1) << 1 | (long)(value & 0x1);

            return Compress(value >> 1, mask >> 1);
        }
    }
}

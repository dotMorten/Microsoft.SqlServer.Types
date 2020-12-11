using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.SqlServer.Types.SqlHierarchy
{
    internal static class KnownPatterns
    {
        //http://www.adammil.net/blog/v100_how_the_SQL_Server_hierarchyid_data_type_works_kind_of_.html
        private static BitPattern[] PositivePatterns = new[]
        {
            new BitPattern(0, 3, "01xxT"),
            new BitPattern(4, 7, "100xxT"),
            new BitPattern(8, 15, "101xxxT"),
            new BitPattern(16, 79, "110xx0x1xxxT"),
            new BitPattern(80, 1103, "1110xxx0xxx0x1xxxT"),
            new BitPattern(1104, 5199, "11110xxxxx0xxx0x1xxxT"),
            new BitPattern(5200, 4294972495, "111110xxxxxxxxxxxxxxxxxxx0xxxxxx0xxx0x1xxxT"),
        };

        private static BitPattern[] NegativePatterns = new[]
        {
            new BitPattern(-8, -1, "00111xxxT"),
            new BitPattern(-72, -9, "0010xx0x1xxxT"),
            new BitPattern(-4168, -73, "000110xxxxx0xxx0x1xxxT"),
            new BitPattern(-4294971464, -4169, "000101xxxxxxxxxxxxxxxxxxx0xxxxxx0xxx0x1xxxT"),
        };

        internal static BitPattern RandomPattern(Random r)
        {
            var index = r.Next(PositivePatterns.Length + NegativePatterns.Length);

            return index < PositivePatterns.Length ? PositivePatterns[index] :
                NegativePatterns[index - PositivePatterns.Length];
        }

        internal static BitPattern GetPatternByValue(long value)
        {
            if (value >= 0)
            {
                foreach (var p in PositivePatterns)
                {
                    if (p.ContainsValue(value))
                        return p;
                }

                throw new InvalidCastException("No pattern found for value:" + value);
            }
            else
            {
                foreach (var p in NegativePatterns)
                {
                    if(p.ContainsValue(value))
                        return p;
                }

                throw new InvalidCastException("No pattern found for value:" + value);
            }
        }

        internal static BitPattern? GetPatternByPrefix(BitReader bitR)
        {
            var remaining = bitR.Remaining;

            if (remaining == 0)
                return null;

            if (remaining < 8 && bitR.Peek(remaining) == 0)
                return null;

            if (bitR.Peek(2) == 0)
            {
                foreach (var pattern in NegativePatterns)
                {
                    if (pattern.BitLength > remaining)
                        break;

                    if (pattern.PrefixOnes == bitR.Peek(pattern.PrefixBitLength))
                        return pattern;
                }

                throw new InvalidCastException("No pattern found for: " + Convert.ToString((int)bitR.Peek(remaining), 2).PadLeft(remaining, '0'));
            }
            else
            {
                foreach (var pattern in PositivePatterns)
                {
                    if (pattern.BitLength > remaining)
                        break;

                    if (pattern.PrefixOnes == bitR.Peek(pattern.PrefixBitLength))
                        return pattern;
                }

                throw new InvalidCastException("No pattern found for: " + Convert.ToString((int)bitR.Peek(remaining), 2).PadLeft(remaining, '0'));
            }
        }
    }
}

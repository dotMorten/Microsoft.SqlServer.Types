using System;
using System.IO;
using System.Linq;

namespace Microsoft.SqlServer.Types
{
    internal class BitReader
    {
        byte[] bytes;
        int bitPosition;

        public int Remaining => bytes.Length * 8 - BitPosition;

        public int BitPosition => bitPosition;

        public BitReader(BinaryReader r)
        {
            bytes = r.BaseStream.ReadAllBytes();
        }

        public ulong Read(int numBits)
        {
            var result = Peek(numBits);
            bitPosition += numBits;
            return result;
        }

        public ulong Peek(int numBits)
        {
            if (numBits == 0)
                return 0;

            var currentByte = BitPosition / 8;
            var newByte = (BitPosition + numBits - 1) / 8;

            if(currentByte == newByte)
            {
                var offset = (8 - BitPosition % 8) - numBits;

                var mask = (ulong)0xFF >> (8 - numBits) << offset;

                return (ulong)(bytes[currentByte] & mask) >> offset;
            }
            else
            {
                ulong result = 0;

                var startOffset = BitPosition % 8;
                var firstCompleteByte = startOffset == 0 ? currentByte : currentByte + 1;
                var endOffset = (BitPosition + numBits) % 8;
                var lastCompleteByte = endOffset == 0 ? newByte + 1 : newByte;

                if (startOffset > 0)
                {
                    var startMask = (ulong)0xFF >> startOffset;

                    result = (bytes[currentByte] & startMask);
                }

                for (int i = firstCompleteByte; i < lastCompleteByte; i++)
                {
                    result = result << 8 | bytes[i];
                }
            
                if (endOffset > 0)
                {
                    var endMastk = (ulong)(0xFF >> (8 - endOffset)) << (8 - endOffset);

                    result = result << endOffset | (ulong)(endMastk & bytes[newByte]) >> (8 - endOffset);
                }

                return result;
            }
        }

        public override string ToString()
        {
            var result = string.Join(" ", this.bytes.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')));
            result = result.Insert(bitPosition + (bitPosition / 8), "|");
            return result;
        }
    }

    internal static class StreamExtensions
    {
        public static byte[] ReadAllBytes(this Stream stream)
        {
            var length = (int)stream.Length;
            var result = new byte[length];
            stream.Read(result, 0, length);
            return result;
        }
    }
}

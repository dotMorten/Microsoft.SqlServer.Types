using System.Linq;

namespace Microsoft.SqlServer.Types
{
    internal class BitReader
    {
        private byte[] _bytes;
        private int _bitPosition;

        public int Remaining => _bytes.Length * 8 - BitPosition;

        public int BitPosition => _bitPosition;

        internal BitReader(BinaryReader r)
        {
            var stream = r.BaseStream;
            var length = (int)stream.Length;
            _bytes = new byte[length];
            stream.Read(_bytes, 0, length);
        }

        internal ulong Read(int numBits)
        {
            var result = Peek(numBits);
            _bitPosition += numBits;
            return result;
        }

        internal ulong Peek(int numBits)
        {
            if (numBits == 0)
                return 0;

            var currentByte = BitPosition / 8;
            var newByte = (BitPosition + numBits - 1) / 8;

            if(currentByte == newByte)
            {
                var offset = (8 - BitPosition % 8) - numBits;

                var mask = (ulong)0xFF >> (8 - numBits) << offset;

                return (ulong)(_bytes[currentByte] & mask) >> offset;
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

                    result = (_bytes[currentByte] & startMask);
                }

                for (int i = firstCompleteByte; i < lastCompleteByte; i++)
                {
                    result = result << 8 | _bytes[i];
                }
            
                if (endOffset > 0)
                {
                    var endMastk = (ulong)(0xFF >> (8 - endOffset)) << (8 - endOffset);

                    result = result << endOffset | (ulong)(endMastk & _bytes[newByte]) >> (8 - endOffset);
                }

                return result;
            }
        }

        public override string ToString()
        {
            var result = string.Join(" ", _bytes.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')));
            return result.Insert(_bitPosition + (_bitPosition / 8), "|");
        }
    }
}

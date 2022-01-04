namespace Microsoft.SqlServer.Types
{
    internal class BitWriter
    {
        private BinaryWriter _writer;
        private byte _nextByte;
        private int _nextLength;

        public BitWriter(BinaryWriter w)
        {
            _writer = w;
        }

        public void Write(ulong value, int valueLength)
        {
            int nextByteRemaining = (8 - _nextLength);

            int pos = 0; 
            for (; pos <= valueLength - 8; pos += 8)
            {
                int valueOffset = valueLength - pos - 8;
                byte b = (byte)((((ulong)0xFF << valueOffset) & value) >> valueOffset);
                
                byte mayorPart = (byte)(b >> _nextLength);
                byte minorPart = (byte)(b << nextByteRemaining);

                _writer.Write((byte)(_nextByte | mayorPart));

                _nextByte = minorPart;
            }
            
            var remainingLength = valueLength - pos;

            var remainingByte = (byte)(value & (ushort)(0xFF >> (8 - remainingLength)));

            var diff = 8 - (_nextLength + remainingLength);
            if (diff > 0)
            {
                _nextByte |= (byte)(remainingByte << diff);
                _nextLength += remainingLength;
            }
            else if (diff == 0)
            {
                _writer.Write((byte)(_nextByte | remainingByte));
                _nextLength = 0;
                _nextByte = 0;
            }
            else if (diff < 0)
            {
                //Finish Byte

                byte mayorPart = (byte)(remainingByte >> -diff);
                _writer.Write((byte)(_nextByte | mayorPart));


                //Create new Byte
                _nextByte = (byte)(remainingByte << (8 + diff));
                _nextLength = -diff;
            }
        }

        public void Finish()
        {
            if (_nextLength > 0)
                _writer.Write((byte)_nextByte);
        }
    }
}

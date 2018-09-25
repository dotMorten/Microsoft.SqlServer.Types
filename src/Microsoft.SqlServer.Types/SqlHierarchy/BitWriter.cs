using System;
using System.IO;

namespace Microsoft.SqlServer.Types
{
    internal class BitWriter
    {
        BinaryWriter writer;
        byte nextByte;
        int nextLength;
        public BitWriter(BinaryWriter w)
        {
            this.writer = w;
        }

        public void Write(ulong value, int valueLength)
        {
            int nextByteRemaining = (8 - nextLength);

            int pos = 0; 
            for (; pos <= valueLength - 8; pos += 8)
            {
                int valueOffset = valueLength - pos - 8;
                byte b = (byte)((((ulong)0xFF << valueOffset) & value) >> valueOffset);
                
                byte mayorPart = (byte)(b >> nextLength);
                byte minorPart = (byte)(b << nextByteRemaining);

                writer.Write((byte)(nextByte | mayorPart));

                nextByte = minorPart;
            }
            
            var remainingLength = valueLength - pos;

            var remainingByte = (byte)(value & (ushort)(0xFF >> (8 - remainingLength)));

            var diff = 8 - (nextLength + remainingLength);
            if (diff > 0)
            {
                nextByte |= (byte)(remainingByte << diff);
                nextLength += remainingLength;
            }
            else if (diff == 0)
            {
                writer.Write((byte)(nextByte | remainingByte));
                nextLength = 0;
                nextByte = 0;
            }
            else if (diff < 0)
            {
                //Finish Byte

                byte mayorPart = (byte)(remainingByte >> -diff);
                writer.Write((byte)(nextByte | mayorPart));


                //Create new Byte
                nextByte = (byte)(remainingByte << (8 + diff));
                nextLength = -diff;
            }
        }

        public void Finish()
        {
            if (nextLength > 0)
                writer.Write((byte)nextByte);
        }
    }
}

using Microsoft.SqlServer.Types;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace src
{
    /// <summary>
    /// Deserialize tests based on examples in the UDT specification
    /// </summary>
    public class BitReadWriteTest
    {
        [Fact]
        public void TestBitWriter()
        {
            using(MemoryStream ms = new MemoryStream())
            {
                using(BinaryWriter byteW = new BinaryWriter(ms))
                {
                    BitWriter bw = new BitWriter(byteW);

                    bw.Write(0b0, 0);
                    bw.Write(0b1, 1);
                    bw.Write(0b00, 2);
                    bw.Write(0b111, 3);

                    bw.Write(0b0000, 4);
                    bw.Write(0b11111, 5);
                    bw.Write(0b000000, 6);
                    bw.Write(0b1111111, 7);
                    bw.Write(0b00000000, 8);
                    bw.Write(0b111111111, 9);
                    bw.Write(0b0000000000, 10);
                    bw.Write(0b11111111111, 11);
                    bw.Finish();
                }

                var result = string.Join(" ", ms.ToArray().Select(b => Convert.ToString(b, 2).PadLeft(8, '0')));

                Assert.Equal("10011100 00111110 00000111 11110000 00001111 11111000 00000001 11111111 11000000", result);
            }
        }

        [Fact]
        public void TestBitReader()
        {
            byte[] array = "10011100 00111110 00000111 11110000 00001111 11111000 00000001 11111111 11000000".Split(" ").Select(a => (byte)Convert.ToInt32(a, 2)).ToArray();

            using (MemoryStream ms = new MemoryStream(array))
            using (BinaryReader byteR = new BinaryReader(ms))
            {
                BitReader br = new BitReader(byteR);

                Assert.Equal(0b0, (int)br.Read(0));
                Assert.Equal(0b1, (int)br.Read(1));
                Assert.Equal(0b00, (int)br.Read(2));
                Assert.Equal(0b111, (int)br.Read(3));
                Assert.Equal(0b0000, (int)br.Read(4));
                Assert.Equal(0b11111, (int)br.Read(5));
                Assert.Equal(0b000000, (int)br.Read(6));
                Assert.Equal(0b1111111, (int)br.Read(7));
                Assert.Equal(0b00000000, (int)br.Read(8));
                Assert.Equal(0b111111111, (int)br.Read(9));
                Assert.Equal(0b0000000000, (int)br.Read(10));
                Assert.Equal(0b11111111111, (int)br.Read(11));

                Assert.Equal(0, (int)br.Read(br.Remaining));
                
                var result = string.Join(" ", ms.ToArray().Select(b => Convert.ToString(b, 2).PadLeft(8, '0')));
            }
        }
    }
}

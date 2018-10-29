using System;
using System.IO;
using System.Linq;

namespace Microsoft.SqlServer.Types.Tests
{
    internal static class StreamExtensions
    {
        public static string ToBinaryString(this byte[] bytes)
        {
            var result = string.Join(" ", bytes.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')));
            return result;
        }

        public static byte[] CreateBytes(params object[] data)
        {
            using (var ms = new MemoryStream())
            {
                System.IO.BinaryWriter bw = new System.IO.BinaryWriter(ms);
                foreach (var item in data)
                {
                    if (item is byte b)
                        bw.Write(b);
                    else if (item is int i)
                        bw.Write(i);
                    else if (item is double d)
                        bw.Write(d);
                    else
                        throw new ArgumentException();
                }
                return ms.ToArray();
            }
        }

    }
}

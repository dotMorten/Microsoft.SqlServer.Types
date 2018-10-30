using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Microsoft.SqlServer.Types.Wkt;
using System;
using System.Linq;
using System.Text;

namespace WKTBenchMark
{
    class Program
    {
        static void Main(string[] args)
        {
             var wkt = System.IO.File.ReadAllLines("data.wkt");
             for (int i = 0; i < wkt.Length; i++)
             {
                var bytes = Encoding.UTF8.GetBytes(wkt[i]);
                var shape = WktReaderSpanByte.Parse(bytes, CoordinateOrder.XY);
            }
            //ReadWkt();
            BenchmarkRunner.Run<BenchmarkTests>();
            Console.ReadKey();
        }
    }

    public class BenchmarkTests
    {
        byte[][] data;
        string[] wkt;

        [GlobalSetup]
        public void Setup()
        {
            wkt = System.IO.File.ReadAllLines("data.wkt");
            data = wkt.Select(w => Encoding.UTF8.GetBytes(w)).ToArray();
        }

        [Benchmark]
        public void String_WKTParser()
        {
            for (int i = 0; i < wkt.Length; i++)
            {
                var shape = WktReader.Parse(wkt[i], CoordinateOrder.XY);
            }
        }

        [Benchmark]
        public void SpanOfByte_WKTParser()
        {
            for (int i = 0; i < wkt.Length; i++)
            {
                var shape = WktReaderSpanByte.Parse(data[i], CoordinateOrder.XY);
            }
        }

        [Benchmark]
        public void SpanOfChar_WKTParser()
        {
            for (int i = 0; i < wkt.Length; i++)
            {
                var shape = WktReaderSpanChar.Parse(wkt[i], CoordinateOrder.XY);
            }
        }
    }
}

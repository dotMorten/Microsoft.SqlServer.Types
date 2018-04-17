using System;
using System.IO;
using Xunit;

namespace src
{
    /// <summary>
    /// Deserialize tests based on examples in the UDT specification
    /// </summary>
    public class DeserializeSpecTests
    {
        [Fact]
        public void TestEmptyPoint()
        {
            var d = double.NaN;
            var bits = BitConverter.DoubleToInt64Bits(d);
            bool isFinite = (bits & 0x7FFFFFFFFFFFFFFF) < 0x7FF0000000000000;
            var emptyPoint = CreateBytes(0, (byte)0x01, (byte)0x04, 0, 0, 1, -1, -1, (byte)0x01);
            var g = Microsoft.SqlServer.Types.SqlGeometry.Deserialize(new System.Data.SqlTypes.SqlBytes(emptyPoint));
            Assert.False(g.IsNull);
            Assert.Equal("Point" , g.STGeometryType().Value);
            Assert.Equal(0, g.STSrid.Value);
            Assert.True(g.STX.IsNull);
            Assert.True(g.STY.IsNull);
            Assert.True(g.M.IsNull);
            Assert.True(g.Z.IsNull);
            Assert.Equal(0, g.STNumGeometries().Value);
        }

        [Fact]
        public void TestPoint()
        {
            var point = CreateBytes(4326, (byte)0x01, (byte)0x0C, 5d, 10d);
            var g = Microsoft.SqlServer.Types.SqlGeometry.Deserialize(new System.Data.SqlTypes.SqlBytes(point));
            Assert.False(g.IsNull);
            Assert.Equal("Point", g.STGeometryType().Value);
            Assert.Equal(4326, g.STSrid.Value);
            Assert.Equal(5, g.STX.Value);
            Assert.Equal(10, g.STY.Value);
            Assert.False(g.HasZ);
            Assert.False(g.HasM);
            Assert.Equal(1, g.STNumGeometries().Value);
        }

        [Fact]
        public void TestLineString()
        {
            var line = CreateBytes(4326, (byte)0x01, (byte)0x05,
                3, 0d, 1d, 3d, 2d, 4d, 5d, 1d, 2d, double.NaN, //vertices
                1, (byte)0x01, 0, //figures
                1, -1, 0, (byte)0x02 //shapes
                );
            var g = Microsoft.SqlServer.Types.SqlGeometry.Deserialize(new System.Data.SqlTypes.SqlBytes(line));
            Assert.False(g.IsNull);
            Assert.Equal("LineString", g.STGeometryType().Value);
            Assert.Equal(4326, g.STSrid.Value);
            Assert.True(g.STX.IsNull);
            Assert.True(g.STY.IsNull);
            Assert.Equal(3, g.STNumPoints().Value);
            Assert.True(g.HasZ);
            Assert.False(g.HasM);
            Assert.Equal(1, g.STNumGeometries().Value);

            Assert.Equal(0d, g.STPointN(1).STX.Value);
            Assert.Equal(1d, g.STPointN(1).STY.Value);
            Assert.Equal(1d, g.STPointN(1).Z.Value);
            Assert.True(g.STPointN(1).M.IsNull);

            Assert.Equal(3d, g.STPointN(2).STX.Value);
            Assert.Equal(2d, g.STPointN(2).STY.Value);
            Assert.Equal(2d, g.STPointN(2).Z.Value);
            Assert.True(g.STPointN(2).M.IsNull);

            var p3 = g.STPointN(3);
            Assert.Equal(4d, p3.STX.Value);
            Assert.Equal(5d, p3.STY.Value);
            Assert.True(p3.HasZ);
            Assert.True(p3.Z.IsNull); //3rd vertex is NaN and should therefore return Null here
            Assert.False(p3.HasM);
            Assert.True(p3.M.IsNull);
        }

        [Fact]
        public void TestGeometryCollection()
        {
            var coll = CreateBytes(4326, (byte)0x01, (byte)0x04,
               13, 0d, 4d, 2d,4d,3d,5d,0d,0d,0d,3d,3d,3d,3d,0d,0d,0d,1d,1d,2d,1d,2d,2d,1d,2d,1d,1d, //vertices
               4, (byte)0x01, 0, (byte)0x01, 1, (byte)0x02, 3, (byte)0x00, 8, //figures
               4, -1, 0, (byte) 0x07, 0,0, (byte)0x01, 0, 1, (byte)0x02, 0, 2, (byte)0x03 //shapes
               );
            var g = Microsoft.SqlServer.Types.SqlGeometry.Deserialize(new System.Data.SqlTypes.SqlBytes(coll));
            Assert.False(g.IsNull);
            Assert.Equal("GeometryCollection", g.STGeometryType().Value);
            Assert.Equal(4326, g.STSrid.Value);
            Assert.True(g.STX.IsNull);
            Assert.True(g.STY.IsNull);
            Assert.Equal(3, g.STNumGeometries());
            var p = g.STGeometryN(1);
            Assert.Equal("Point", p.STGeometryType());
            Assert.Equal(0d, p.STX.Value);
            Assert.Equal(4d, p.STY.Value);
            var l = g.STGeometryN(2);
            Assert.Equal("LineString", l.STGeometryType());
            Assert.Equal(2, l.STNumPoints());

            var pg = g.STGeometryN(3);
            Assert.Equal("Polygon", pg.STGeometryType());
            Assert.Equal(10, pg.STNumPoints());
            var extRing = pg.STExteriorRing();
            Assert.False(extRing.IsNull);
            Assert.Equal(5, extRing.STNumPoints());
            Assert.Equal(1, pg.STNumInteriorRing().Value);
            Assert.Equal(5, pg.STInteriorRingN(1).STNumPoints());
        }

        [Fact]
        public void TestCurvePolygon()
        {
            //TODO: Curve support not complete
            var coll = CreateBytes(4326, (byte)0x02, (byte)0x24,
              5, 0d,0d,2d,0d,2d,2d,0d,1d,0d,0d, //vertices
              1, (byte)0x03, 0, //figures
              1, -1, 0, (byte)0x10, //shapes
              3, (byte)0x02, (byte)0x00, (byte)0x03 //Segments
              );

            var g = Microsoft.SqlServer.Types.SqlGeometry.Deserialize(new System.Data.SqlTypes.SqlBytes(coll));
            Assert.False(g.IsNull);
            Assert.Equal("CURVEPOLYGON", g.STGeometryType().Value);
            Assert.Equal(4326, g.STSrid.Value);
            //TODO More asserts here
        }

        private static byte[] CreateBytes(params object[] data)
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

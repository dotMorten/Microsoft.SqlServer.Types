using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Microsoft.SqlServer.Types.Tests.Geometry
{
    [TestClass]
    [TestCategory("SqlGeometry")]
    public class DeserializeFromBinary
    {
        [TestMethod]
        public void TestEmptyPoint()
        {
            var d = double.NaN;
            var bits = BitConverter.DoubleToInt64Bits(d);
            bool isFinite = (bits & 0x7FFFFFFFFFFFFFFF) < 0x7FF0000000000000;
            var emptyPoint = StreamExtensions.CreateBytes(0, (byte)0x01, (byte)0x04, 0, 0, 1, -1, -1, (byte)0x01);
            var g = Microsoft.SqlServer.Types.SqlGeometry.Deserialize(new System.Data.SqlTypes.SqlBytes(emptyPoint));
            Assert.IsFalse(g.IsNull);
            Assert.AreEqual("Point", g.STGeometryType().Value);
            Assert.AreEqual(0, g.STSrid.Value);
            Assert.IsTrue(g.STX.IsNull);
            Assert.IsTrue(g.STY.IsNull);
            Assert.IsTrue(g.M.IsNull);
            Assert.IsTrue(g.Z.IsNull);
            Assert.AreEqual(0, g.STNumGeometries().Value);
        }

        [TestMethod]
        public void TestPoint()
        {
            var point = StreamExtensions.CreateBytes(4326, (byte)0x01, (byte)0x0C, 5d, 10d);
            var g = Microsoft.SqlServer.Types.SqlGeometry.Deserialize(new System.Data.SqlTypes.SqlBytes(point));
            Assert.IsFalse(g.IsNull);
            Assert.AreEqual("Point", g.STGeometryType().Value);
            Assert.AreEqual(4326, g.STSrid.Value);
            Assert.AreEqual(5, g.STX.Value);
            Assert.AreEqual(10, g.STY.Value);
            Assert.IsFalse(g.HasZ);
            Assert.IsFalse(g.HasM);
            Assert.AreEqual(1, g.STNumGeometries().Value);
        }

        [TestMethod]
        public void TestLineString()
        {
            var line = StreamExtensions.CreateBytes(4326, (byte)0x01, (byte)0x05,
                3, 0d, 1d, 3d, 2d, 4d, 5d, 1d, 2d, double.NaN, //vertices
                1, (byte)0x01, 0, //figures
                1, -1, 0, (byte)0x02 //shapes
                );
            var g = Microsoft.SqlServer.Types.SqlGeometry.Deserialize(new System.Data.SqlTypes.SqlBytes(line));
            Assert.IsFalse(g.IsNull);
            Assert.AreEqual("LineString", g.STGeometryType().Value);
            Assert.AreEqual(4326, g.STSrid.Value);
            Assert.IsTrue(g.STX.IsNull);
            Assert.IsTrue(g.STY.IsNull);
            Assert.AreEqual(3, g.STNumPoints().Value);
            Assert.IsTrue(g.HasZ);
            Assert.IsFalse(g.HasM);
            Assert.AreEqual(1, g.STNumGeometries().Value);

            Assert.AreEqual(0d, g.STPointN(1).STX.Value);
            Assert.AreEqual(1d, g.STPointN(1).STY.Value);
            Assert.AreEqual(1d, g.STPointN(1).Z.Value);
            Assert.IsTrue(g.STPointN(1).M.IsNull);

            Assert.AreEqual(3d, g.STPointN(2).STX.Value);
            Assert.AreEqual(2d, g.STPointN(2).STY.Value);
            Assert.AreEqual(2d, g.STPointN(2).Z.Value);
            Assert.IsTrue(g.STPointN(2).M.IsNull);

            var p3 = g.STPointN(3);
            Assert.AreEqual(4d, p3.STX.Value);
            Assert.AreEqual(5d, p3.STY.Value);
            Assert.IsFalse(p3.HasZ);
            Assert.IsTrue(p3.Z.IsNull); //3rd vertex is NaN and should therefore return Null here
            Assert.IsFalse(p3.HasM);
            Assert.IsTrue(p3.M.IsNull);
        }

        [TestMethod]
        public void TestGeometryCollection()
        {
            var coll = StreamExtensions.CreateBytes(4326, (byte)0x01, (byte)0x04,
               13, 0d, 4d, 2d, 4d, 3d, 5d, 0d, 0d, 0d, 3d, 3d, 3d, 3d, 0d, 0d, 0d, 1d, 1d, 2d, 1d, 2d, 2d, 1d, 2d, 1d, 1d, //vertices
               4, (byte)0x01, 0, (byte)0x01, 1, (byte)0x02, 3, (byte)0x00, 8, //figures
               4, -1, 0, (byte)0x07, 0, 0, (byte)0x01, 0, 1, (byte)0x02, 0, 2, (byte)0x03 //shapes
               );
            var g = Microsoft.SqlServer.Types.SqlGeometry.Deserialize(new System.Data.SqlTypes.SqlBytes(coll));
            Assert.IsFalse(g.IsNull);
            Assert.AreEqual("GeometryCollection", g.STGeometryType().Value);
            Assert.AreEqual(4326, g.STSrid.Value);
            Assert.IsTrue(g.STX.IsNull);
            Assert.IsTrue(g.STY.IsNull);
            Assert.AreEqual(3, g.STNumGeometries());
            var p = g.STGeometryN(1);
            Assert.AreEqual("Point", p.STGeometryType());
            Assert.AreEqual(0d, p.STX.Value);
            Assert.AreEqual(4d, p.STY.Value);
            var l = g.STGeometryN(2);
            Assert.AreEqual("LineString", l.STGeometryType());
            Assert.AreEqual(2, l.STNumPoints());

            var pg = g.STGeometryN(3);
            Assert.AreEqual("Polygon", pg.STGeometryType());
            Assert.AreEqual(10, pg.STNumPoints());
            var extRing = pg.STExteriorRing();
            Assert.IsFalse(extRing.IsNull);
            Assert.AreEqual(5, extRing.STNumPoints());
            Assert.AreEqual(1, pg.STNumInteriorRing().Value);
            Assert.AreEqual(5, pg.STInteriorRingN(1).STNumPoints());
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void TestCurvePolygon()
        {
            //TODO: Curve support not complete
            var coll = StreamExtensions.CreateBytes(4326, (byte)0x02, (byte)0x24,
              5, 0d, 0d, 2d, 0d, 2d, 2d, 0d, 1d, 0d, 0d, //vertices
              1, (byte)0x03, 0, //figures
              1, -1, 0, (byte)0x10, //shapes
              3, (byte)0x02, (byte)0x00, (byte)0x03 //Segments
              );

            var g = Microsoft.SqlServer.Types.SqlGeometry.Deserialize(new System.Data.SqlTypes.SqlBytes(coll));
            Assert.IsFalse(g.IsNull);
            Assert.AreEqual("CURVEPOLYGON", g.STGeometryType().Value);
            Assert.AreEqual(4326, g.STSrid.Value);
            //TODO More asserts here
        }
    }
}

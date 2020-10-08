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
        public void TestPolygonFromHexString()
        {
            var s = "0000000001041B000000C5F8D8708E885EC01C812C5F17D24740C57CD7938E885EC05C5D8C9918D24740C5406ACA88885EC08462D48918D24740C6EAC19B88885EC0BCB3D0DA06D24740C418796C87885EC05C8A60BB06D24740C49C778F87885EC04011C2EC09D24740C5348AC885885EC0E82B0ADD09D24740C5348AC885885EC058CB0AD40BD24740C6B037897D885EC0B0C3E2A40BD24740C47AE44E7D885EC034BDDEC803D24740C5DAD4CF7E885EC0347926B903D24740C4067FDB7E885EC080D9732900D24740C4DEB78B81885EC078ECE34800D24740C5580D8081885EC0D4395FF1F3D14740C668A62F83885EC0F8AEF448F2D14740C4E4D11A87885EC038009C7CF1D14740C4E4D11A87885EC0BCD115DD02D24740C626663389885EC0DCC1A5BD02D24740C5AA645689885EC0381B7C9700D24740C5D255B48A885EC064323B1EFFD14740C4CE653891885EC0E4CC5ADFFED14740C5F8027F92885EC0AC8514E600D24740C4B0A8F392885EC0E8DD4FDF05D24740C6CD0F4491885EC09032301E06D24740C47F645B91885EC0E40952CD09D24740C5F8D8708E885EC014E4E1AD09D24740C5F8D8708E885EC01C812C5F17D2474001000000020000000001000000FFFFFFFF0000000003";
            var g = Microsoft.SqlServer.Types.SqlGeometry.Deserialize(new System.Data.SqlTypes.SqlBytes(s.HexStringToByteArray()));
            Assert.IsFalse(g.IsNull);
            Assert.AreEqual("Polygon", g.STGeometryType().Value);
            Assert.AreEqual(0, g.STSrid.Value);
            Assert.IsTrue(g.STX.IsNull);
            Assert.IsTrue(g.STY.IsNull);
            Assert.AreEqual(27, g.STNumPoints().Value);
            Assert.IsFalse(g.HasZ);
            Assert.IsFalse(g.HasM);
            Assert.AreEqual(1, g.STNumGeometries().Value);

            Assert.AreEqual(47.641338249903328, g.STPointN(1).STY.Value);
            Assert.AreEqual(-122.13369389713905, g.STPointN(1).STX.Value);
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
        public void TestEmptyGeometryCollection()
        {
            var coll = StreamExtensions.CreateBytes(4326, (byte)0x01, (byte)0x04,
                0, //vertices
                0, //figures
                1, -1, -1, (byte)0x07 //shapes
            );
            var g = Microsoft.SqlServer.Types.SqlGeometry.Deserialize(new System.Data.SqlTypes.SqlBytes(coll));
            Assert.IsFalse(g.IsNull);
            Assert.AreEqual("GeometryCollection", g.STGeometryType().Value);
            Assert.AreEqual(4326, g.STSrid.Value);
            Assert.IsTrue(g.STX.IsNull);
            Assert.IsTrue(g.STY.IsNull);
            Assert.AreEqual(0, g.STNumGeometries());
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

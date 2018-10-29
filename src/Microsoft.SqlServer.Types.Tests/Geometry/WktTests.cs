using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.SqlServer.Types.Tests.Geometry
{
    [TestClass]
    public class WktTests
    {
        public object StreamExtensions { get; private set; }

        [TestMethod]
        public void TestNullToString()
        {
            var str = SqlGeometry.Null.ToString();
            Assert.AreEqual("Null", str);
        }

        [TestMethod]
        public void TestPointToString()
        {
            var point = Tests.StreamExtensions.CreateBytes(4326, (byte)0x01, (byte)0x0C, 5d, 10d);
            var g = Microsoft.SqlServer.Types.SqlGeometry.Deserialize(new System.Data.SqlTypes.SqlBytes(point));
            var str = g.ToString();
            Assert.AreEqual("POINT (5 10)", str);
        }
        [TestMethod]
        public void TestPointFromString()
        {
            var g = Microsoft.SqlServer.Types.SqlGeometry.Parse(new System.Data.SqlTypes.SqlString("POINT (5 10)"));
            Assert.AreEqual(0, g.STSrid.Value);
            Assert.AreEqual(5, g.STX.Value);
            Assert.AreEqual(10, g.STY.Value);
            Assert.IsFalse(g.HasZ);
            Assert.IsFalse(g.HasM);
        }

        [TestMethod]
        public void TestLineStringToString()
        {
            var line = Tests.StreamExtensions.CreateBytes(4326, (byte)0x01, (byte)0x05,
                3, 0d, 1d, 3d, 2d, 4d, 5d, 1d, 2d, double.NaN, //vertices
                1, (byte)0x01, 0, //figures
                1, -1, 0, (byte)0x02 //shapes
                );
            var g = Microsoft.SqlServer.Types.SqlGeometry.Deserialize(new System.Data.SqlTypes.SqlBytes(line));
            var str = g.ToString();
            Assert.AreEqual("LINESTRING (0 1 1, 3 2 2, 4 5)", str);
        }
        [TestMethod]
        public void TestLineStringFromString()
        {
            var g = SqlGeometry.Parse("LINESTRING (0 1 1, 3 2 2, 4 5)");
            Assert.IsFalse(g.IsNull);
            Assert.AreEqual("LineString", g.STGeometryType().Value);
            Assert.AreEqual(0, g.STSrid.Value);
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


    }
}

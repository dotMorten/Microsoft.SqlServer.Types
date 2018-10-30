using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.SqlServer.Types.Tests.Geometry
{
    [TestClass]
    [TestCategory("SqlGeometry")]
    [TestCategory("WKT")]
    public class WktTests
    {
        [TestMethod]
        public void NullToString()
        {
            var str = SqlGeometry.Null.ToString();
            Assert.AreEqual("Null", str);
        }

        [TestMethod]
        public void PointToString()
        {
            var point = Tests.StreamExtensions.CreateBytes(4326, (byte)0x01, (byte)0x0C, 5d, 10d);
            var g = Microsoft.SqlServer.Types.SqlGeometry.Deserialize(new System.Data.SqlTypes.SqlBytes(point));
            var str = g.ToString();
            Assert.AreEqual("POINT (5 10)", str);
        }

        [TestMethod]
        public void PointFromString()
        {
            var g = Microsoft.SqlServer.Types.SqlGeometry.Parse(new System.Data.SqlTypes.SqlString("POINT (5 10)"));
            Assert.AreEqual(0, g.STSrid.Value);
            Assert.AreEqual(5, g.STX.Value);
            Assert.AreEqual(10, g.STY.Value);
            Assert.IsFalse(g.HasZ);
            Assert.IsFalse(g.HasM);
        }

        [TestMethod]
        public void LineStringToString()
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
        public void LineStringFromString()
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


        [DataTestMethod]
        [DataRow("POINT")]
        [DataRow("MULTIPOINT")]
        [DataRow("LINESTRING")]
        [DataRow("MULTILINESTRING")]
        [DataRow("POLYGON")]
        [DataRow("MULTIPOLYGON")]
        [DataRow("GEOMETRYCOLLECTION")]
        public void EmptyGeometriesFromString(string parameter)
        {
            var g = SqlGeometry.Parse(parameter + " EMPTY");
            Assert.IsFalse(g.IsNull);
            Assert.AreEqual(parameter, g.STGeometryType().Value.ToUpper());
            Assert.IsTrue(g.STIsEmpty().Value);
        }

        [TestMethod]
        public void MultiLineStringFromString()
        {
            using (var conn = new System.Data.SqlClient.SqlConnection(DBTests.ConnectionString))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT geometry::Parse('MULTILINESTRING((-10 11, 13 14, 15 16), (20 21, 22 23, 24 25, 26 27))')";
                    var geom = cmd.ExecuteScalar();
                    //Assert.AreEqual(id.ToString(), geom.ToString());
                }
            }


            var g = SqlGeometry.Parse("MULTILINESTRING ((-10 11, 13 14, 15 16), (20 21, 22 23, 24 25, 26 27))");
            Assert.IsFalse(g.IsNull);
            Assert.AreEqual("MultiLineString", g.STGeometryType().Value);
            Assert.AreEqual(0, g.STSrid.Value);
            Assert.IsTrue(g.STX.IsNull);
            Assert.IsTrue(g.STY.IsNull);
            Assert.IsFalse(g.HasZ);
            Assert.IsFalse(g.HasM);
            Assert.AreEqual(2, g.STNumGeometries().Value);

            var part1 = g.STGeometryN(1);
            var part2 = g.STGeometryN(2);
            Assert.AreEqual(3, part1.STNumPoints());
            Assert.AreEqual(4, part2.STNumPoints());

            Assert.AreEqual(-10d, part1.STPointN(1).STX.Value);
            Assert.AreEqual(11d, part1.STPointN(1).STY.Value);
            Assert.AreEqual(3, part1.STNumPoints().Value);
            Assert.IsTrue(part2.STPointN(1).Z.IsNull);
            Assert.IsTrue(part2.STPointN(1).M.IsNull);
            Assert.AreEqual(4, part2.STNumPoints().Value);
        }

        [TestMethod]
        public void PolygonFromString()
        {
            var g = SqlGeometry.Parse("POLYGON((-122.358 47.653, -122.348 47.649, -122.348 47.658, -122.358 47.658, -122.358 47.653))");
            Assert.IsFalse(g.IsNull);
            Assert.AreEqual("Polygon", g.STGeometryType().Value);
            Assert.AreEqual(5, g.STNumPoints().Value);
            Assert.IsFalse(g.HasZ);
            Assert.IsFalse(g.HasM);
            Assert.AreEqual(1, g.STNumGeometries().Value);
        }

        [TestMethod]
        public void MultiPolygonFromString()
        {
            var g = SqlGeometry.Parse("MULTIPOLYGON(((-122.358 47.653, -122.348 47.649, -122.358 47.658, -122.358 47.653)), ((-122.341 47.656, -122.341 47.661, -122.351 47.661, -122.341 47.656)))");
            Assert.IsFalse(g.IsNull);
            Assert.AreEqual("MultiPolygon", g.STGeometryType().Value);
            Assert.AreEqual(8, g.STNumPoints().Value);
            Assert.IsFalse(g.HasZ);
            Assert.IsFalse(g.HasM);
            Assert.AreEqual(2, g.STNumGeometries().Value);
        }

        [TestMethod]
        public void GeometryCollectionFromString()
        {
            var g = SqlGeometry.Parse("GEOMETRYCOLLECTION ( POINT(-122.34900 47.65100), LINESTRING(-122.360 47.656, -122.343 47.656))");
            Assert.IsFalse(g.IsNull);
            Assert.AreEqual("GeometryCollection", g.STGeometryType().Value);
            Assert.AreEqual(2, g.STNumGeometries().Value);
            var g1 = g.STGeometryN(1);
            var g2 = g.STGeometryN(2);
            Assert.AreEqual("Point", g1.STGeometryType());
            Assert.AreEqual("LineString", g2.STGeometryType());
        }

        [TestMethod]
        [WorkItem(13)]
        public void UserSubmittedIssue_WKT1()
        {
            using (var conn = new System.Data.SqlClient.SqlConnection(DBTests.ConnectionString))
            {
                conn.Open();
                var id = SqlGeometry.Parse("LINESTRING (100 100, 20 180, 180 180)");
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT @p";
                    var p =cmd.Parameters.Add("@p", System.Data.SqlDbType.Udt);
                    p.UdtTypeName = "geometry";
                    p.Value = id;
                    Assert.AreEqual(id.ToString(), cmd.ExecuteScalar().ToString());
                }
            }
        }
    }
}

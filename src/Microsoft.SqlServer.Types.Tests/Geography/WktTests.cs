using Microsoft.SqlServer.Types.Tests.Geometry;
using Microsoft.VisualStudio.TestTools.UnitTesting;
#if NETCOREAPP3_1
using Microsoft.Data.SqlClient;
#else
using System.Data.SqlClient;
#endif
using System.Data.SqlTypes;

namespace Microsoft.SqlServer.Types.Tests.Geography
{
    [TestClass]
    [TestCategory("SqlGeography")]
    [TestCategory("WKT")]
    public class WktTests
    {

        [TestMethod]
        [WorkItem(13)]
        public void UserSubmittedIssue_WKT2()
        {
            using (var conn = new SqlConnection(DBTests.ConnectionString))
            {
                conn.Open();
                var id = SqlGeography.Parse("LINESTRING (-122.36 47.656, -122.343 47.656)");

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT @p";
                    var p = cmd.CreateParameter();
                    cmd.Parameters.Add(p);

                    p.UdtTypeName = "geography";
                    p.ParameterName = "@p";
                    p.Value = id;

                    Assert.AreEqual(id.ToString(), cmd.ExecuteScalar().ToString());
                }
            }
        }

        [TestMethod]
        [WorkItem(13)]
        public void UserSubmittedIssue_WKT3()
        {
            using (var conn = new SqlConnection(DBTests.ConnectionString))
            {
                conn.Open();
                var id = SqlGeography.Parse("LINESTRING (-122.36 47.656, -122.343 47.656)");
                var l = id.STPointN(1).Long;
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Cast(geography::STGeomFromText('LINESTRING(-122.360 47.656, -122.343 47.656)', 4326) as geography)";

                    Assert.AreEqual(id.ToString(), cmd.ExecuteScalar().ToString());
                }
            }
        }

        [TestMethod]
        [WorkItem(14)]
        public void UserSubmittedIssue_WKT4()
        {
            using (var conn = new SqlConnection(DBTests.ConnectionString))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Cast(geography::STGeomFromText('LINESTRING(-122.360 47.656, -122.343 47.656)', 4326) as geography)";
                    var result = cmd.ExecuteScalar();
                    Assert.IsInstanceOfType(result, typeof(SqlGeography));
                    SqlGeography g = (SqlGeography)result;
                    Assert.AreEqual("LineString", g.STGeometryType());
                    Assert.AreEqual(2, g.STNumPoints());
                    Assert.AreEqual(47.656, g.STPointN(1).Lat.Value);
                    Assert.AreEqual(-122.360, g.STPointN(1).Long.Value);
                    Assert.AreEqual(47.656, g.STPointN(2).Lat.Value);
                    Assert.AreEqual(-122.343, g.STPointN(2).Long.Value);
                    Assert.AreEqual("LINESTRING (-122.36 47.656, -122.343 47.656)", cmd.ExecuteScalar().ToString());
                }
            }
        }

        [TestMethod]
        public void CreateFullGlobe()
        {
            var wkt = "FULLGLOBE";
            var value = SqlGeography.STGeomFromText(new SqlChars(new SqlString(wkt)), 4326);
            Assert.IsNotNull(value);
        }

        [TestMethod]
        public void FullGlobeWkt()
        {
            var wkt = "FULLGLOBE";
            var value = SqlGeography.STGeomFromText(new SqlChars(new SqlString(wkt)), 4326);

            var valueWkt = value.STAsText().ToSqlString().Value;
            Assert.AreEqual(wkt, valueWkt);
        }

        [TestMethod]
        [ExpectedException(typeof(System.FormatException))]
        public void FailCollectionWithFullGlobe()
        {
            var wkt = "GEOMETRYCOLLECTION (POINT (40 10), LINESTRING (10 10, 20 20, 10 40), FULLGLOBE, POLYGON ((40 40, 20 45, 45 30, 40 40)))";
            var value = SqlGeography.STGeomFromText(new SqlChars(new SqlString(wkt)), 4326);
            var valueWkt = value.STAsText().ToSqlString().Value;
            Assert.AreEqual(wkt, valueWkt);
        }

        [TestMethod]
        public void TestToString()
        {
            var wkt = "POINT (40 10)";
            var value = SqlGeography.STGeomFromText(new SqlChars(new SqlString(wkt)), 4326);
            Assert.AreEqual(wkt, value.ToString());
        }

        [TestMethod]
        [TestCategory("SqlGeography")]
        public void ReadGeometryCollection()
        {
            var p = SqlGeography.Parse("GEOMETRYCOLLECTION (POINT(10 11), LINESTRING(20 30, 20 40), POLYGON EMPTY, GEOMETRYCOLLECTION(POINT(30 31)))");
            Assert.IsNotNull(p);
            Assert.AreEqual("GeometryCollection", p.STGeometryType());
            Assert.AreEqual(4, p.STNumGeometries());
            var g1 = p.STGeometryN(1);
            Assert.AreEqual("Point", g1.STGeometryType());
            Assert.AreEqual(10d, g1.Long.Value);
            Assert.AreEqual(11d, g1.Lat.Value);
            Assert.IsFalse(g1.HasZ);
            Assert.IsFalse(g1.HasM);

            var g2 = p.STGeometryN(2);
            Assert.AreEqual("LineString", g2.STGeometryType());
            Assert.AreEqual(2, g2.STNumPoints());
            Assert.AreEqual(20, g2.STPointN(1).Long);
            Assert.AreEqual(30, g2.STPointN(1).Lat);
            Assert.AreEqual(20, g2.STPointN(2).Long);
            Assert.AreEqual(40, g2.STPointN(2).Lat);

            var g3 = p.STGeometryN(3);
            Assert.AreEqual("Polygon", g3.STGeometryType());
            Assert.IsTrue(g3.STIsEmpty().Value);

            var g4 = p.STGeometryN(4);
            Assert.AreEqual("GeometryCollection", g4.STGeometryType());
            Assert.AreEqual(1, g4.STNumGeometries());
            var g4_1 = g4.STGeometryN(1);
            Assert.AreEqual("Point", g4_1.STGeometryType());
            Assert.AreEqual(30d, g4_1.Long.Value);
            Assert.AreEqual(31d, g4_1.Lat.Value);
        }

        [TestMethod]
        public void ReadPolygonWithEmptyRing()
        {
            AssertEx.ThrowsException(() =>
                SqlGeography.Parse("POLYGON ((10 20, 15 25, 20 30, 10 20), (15 25, 20 30, 25 35, 15 25), EMPTY, (5 5, 6 6, 7 7, 5 5))"),
                typeof(System.FormatException), "24305: The Polygon input is not valid because the ring number 3 does not have enough points. Each ring of a polygon must contain at least four points.");
        }
    }
}

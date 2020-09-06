﻿using Microsoft.SqlServer.Types.Tests.Geometry;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            using (var conn = new System.Data.SqlClient.SqlConnection(DBTests.ConnectionString))
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
            using (var conn = new System.Data.SqlClient.SqlConnection(DBTests.ConnectionString))
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
            using (var conn = new System.Data.SqlClient.SqlConnection(DBTests.ConnectionString))
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
    }
}

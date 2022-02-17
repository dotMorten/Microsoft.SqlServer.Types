using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.SqlServer.Types.Tests
{
    // Tests ported from 
    // https://github.com/OData/odata.net/blob/4c9c57e470cb9b95b9d4fe6f800c5d2c2f11861d/test/FunctionalTests/Microsoft.Spatial.Tests/WellKnownTextSqlFormatterTests.cs

    [TestClass]
    [TestCategory("WKT")]
    public class WellKnownTextSqlFormatterTests
    {
        private class PositionData
        {
            public PositionData(double x, double y) : this(x, y, null, null) { }
            public PositionData(double x, double y, double? z) : this(x, y, z, null) { }
            public PositionData(double x, double y, double? z, double? m) { X = x; Y = y; Z = z; M = m; }
            public double X { get; }
            public double Y { get; }
            public double? Z { get; }
            public double? M { get; }
        }

        [TestMethod]
        public void ReadPoint_Ordering()
        {
            // ensure longitude latitude ordering
            var point = SqlGeography.STGeomFromText(new System.Data.SqlTypes.SqlChars("POINT(10 20)"), 4326);
            Assert.AreEqual(20, point.Lat);
            Assert.AreEqual(10, point.Long);
        }

        [TestMethod]
        public void ReadSRIDTest()
        {
            ReadGeographyPointTest("POINT(10 20)", new PositionData(20, 10), 4326);
        }

        [TestMethod]
        public void NullPipelineTest()
        {
            var point = SqlGeometry.Parse("POINT(12345 567890)");
            Assert.AreEqual(12345, point.STX);
            Assert.AreEqual(567890, point.STY);
        }

        private static void ReadGeographyPointTest(string input, PositionData expected, int? expectedCoordinateSystem = null)
        {
            SqlGeography p;
            if (expectedCoordinateSystem.HasValue)
                p = SqlGeography.STGeomFromText(new System.Data.SqlTypes.SqlChars(input), expectedCoordinateSystem.Value);
            else
                p = SqlGeography.Parse(new System.Data.SqlTypes.SqlString(input));

            Assert.IsNotNull(p);
            VerifyAsPoint(p, expected);

            if (expectedCoordinateSystem != null)
            {
                Assert.AreEqual(expectedCoordinateSystem, p.STSrid.Value);
            }
            else
            {
                Assert.AreEqual(4326, p.STSrid.Value);
            }
        }

        private static void ReadGeometryPointTest(string input, PositionData expected, int? expectedCoordinateSystem = null)
        {
            SqlGeometry p;
            if (expectedCoordinateSystem.HasValue)
                p = SqlGeometry.STGeomFromText(new System.Data.SqlTypes.SqlChars(input), expectedCoordinateSystem.Value);
            else
                p = SqlGeometry.Parse(new System.Data.SqlTypes.SqlString(input));

            Assert.IsNotNull(p);
            VerifyAsPoint(p, expected);

            if (expectedCoordinateSystem != null)
            {
                Assert.AreEqual(expectedCoordinateSystem, p.STSrid.Value);
            }
            else
            {
                Assert.AreEqual(0, p.STSrid.Value);
            }
        }

        private static void VerifyAsPoint(SqlGeography actual, PositionData expected)
        {
            Assert.AreEqual("Point", actual.STGeometryType().Value);
            VerifyPosition(expected, actual);
        }

        private static void VerifyAsPoint(SqlGeometry actual, PositionData expected)
        {
            Assert.AreEqual("Point", actual.STGeometryType().Value);
            VerifyPosition(expected, actual);
        }

        private static void VerifyAsMultiPoint(SqlGeography actual, params PositionData[] expected)
        {
            Assert.AreEqual("MultiPoint", actual.STGeometryType().Value);
            Assert.AreEqual(actual.STNumGeometries(), expected?.Length ?? 0);
            for (int i = 0; i < actual.STNumGeometries(); ++i)
            {
                VerifyAsPoint(actual.STGeometryN(i + 1), expected[i]);
            }
        }

        private static void VerifyAsMultiPoint(SqlGeometry actual, params PositionData[] expected)
        {
            Assert.AreEqual("MultiPoint", actual.STGeometryType().Value);
            Assert.AreEqual(actual.STNumGeometries(), expected?.Length ?? 0);
            for (int i = 0; i < actual.STNumGeometries(); ++i)
            {
                VerifyAsPoint(actual.STGeometryN(i + 1), expected[i]);
            }
        }

        private static void VerifyAsLineString(SqlGeography actual, params PositionData[] expected)
        {
            Assert.AreEqual("LineString", actual.STGeometryType().Value);
            Assert.AreEqual(actual.STNumPoints(), expected?.Length ?? 0);
            for (int i = 0; i < actual.STNumPoints(); ++i)
            {
                VerifyAsPoint(actual.STPointN(i + 1), expected[i]);
            }
        }

        private static void VerifyAsLineString(SqlGeometry actual, params PositionData[] expected)
        {
            Assert.AreEqual("LineString", actual.STGeometryType().Value);
            Assert.AreEqual(actual.STNumPoints(), expected?.Length ?? 0);
            for (int i = 0; i < actual.STNumPoints(); ++i)
            {
                VerifyAsPoint(actual.STPointN(i + 1), expected[i]);
            }
        }

        private static void VerifyAsMultiLineString(SqlGeography actual, params PositionData[][] expected)
        {
            Assert.AreEqual("MultiLineString", actual.STGeometryType().Value);
            Assert.AreEqual(actual.STNumGeometries(), expected?.Length ?? 0);
            for (int i = 0; i < actual.STNumGeometries(); ++i)
            {
                VerifyAsLineString(actual.STGeometryN(i + 1), expected[i]);
            }
        }

        private static void VerifyAsPolygon(SqlGeography actual, params PositionData[][] expected)
        {
            Assert.AreEqual("Polygon", actual.STGeometryType().Value);
            Assert.AreEqual(expected == null ? System.Data.SqlTypes.SqlInt32.Null : expected.Length, actual.STNumCurves());
            for (int i = 0; i < actual.STNumCurves(); ++i)
            {
                // expected[i] can be null
                VerifyAsLineString(actual.STCurveN(i + 1), expected[i]);
            }
        }

        private static void VerifyAsPolygon(SqlGeometry actual, params PositionData[][] expected)
        {
            Assert.AreEqual("Polygon", actual.STGeometryType().Value);
            Assert.AreEqual(actual.STNumCurves(), expected == null ? System.Data.SqlTypes.SqlInt32.Null : expected.Length);
            for (int i = 0; i < actual.STNumCurves(); ++i)
            {
                // expected[i] can be null
                VerifyAsLineString(actual.STCurveN(i + 1), expected[i]);
            }
        }

        private static void VerifyAsMultiPolygon(SqlGeography actual, params PositionData[][][] expected)
        {
            Assert.AreEqual("MultiPolygon", actual.STGeometryType().Value);
            Assert.AreEqual(actual.STNumGeometries(), expected?.Length ?? 0);
            for (int i = 0; i < actual.STNumGeometries(); ++i)
            {
                VerifyAsPolygon(actual.STGeometryN(i + 1), expected[i]);
            }
        }

        private static void VerifyAsMultiPolygon(SqlGeometry actual, params PositionData[][][] expected)
        {
            Assert.AreEqual("MultiPolygon", actual.STGeometryType().Value);
            Assert.AreEqual(actual.STNumGeometries(), expected?.Length ?? 0);
            for (int i = 0; i < actual.STNumGeometries(); ++i)
            {
                VerifyAsPolygon(actual.STGeometryN(i + 1), expected[i]);
            }
        }

        public static void VerifyAsCollection(SqlGeography actual, params Action<SqlGeography>[] verifier)
        {
            Assert.AreEqual("GeometryCollection", actual.STGeometryType().Value);
            for (int i = 0; i < actual.STNumGeometries(); ++i)
            {
                verifier[i](actual.STGeometryN(i + 1));
            }
        }

        public static void VerifyAsCollection(SqlGeometry actual, params Action<SqlGeometry>[] verifier)
        {
            Assert.AreEqual("GeometryCollection", actual.STGeometryType().Value);
            for (int i = 0; i < actual.STNumGeometries(); ++i)
            {
                verifier[i](actual.STGeometryN(i + 1));
            }
        }

        private static void VerifyPosition(PositionData expected, SqlGeography actual)
        {
            if (expected == null)
            {
                Assert.IsTrue(actual.STIsEmpty().Value);
                return;
            }
            Assert.AreEqual(expected.X, actual.Lat);
            Assert.AreEqual(expected.Y, actual.Long);
            if (expected.Z.HasValue)
                Assert.AreEqual(expected.Z.Value, actual.Z.Value);
            else
                Assert.IsTrue(actual.Z.IsNull);
            if (expected.M.HasValue)
                Assert.AreEqual(expected.M.Value, actual.M.Value);
            else
                Assert.IsTrue(actual.M.IsNull);
        }

        private static void VerifyPosition(PositionData expected, SqlGeometry actual)
        {
            if (expected == null)
            {
                Assert.IsTrue(actual.STIsEmpty().Value);
                return;
            }
            Assert.AreEqual(expected.X, actual.STX);
            Assert.AreEqual(expected.Y, actual.STY);
            if (expected.Z.HasValue)
                Assert.AreEqual(expected.Z.Value, actual.Z.Value);
            else
                Assert.IsTrue(actual.Z.IsNull);
            if (expected.M.HasValue)
                Assert.AreEqual(expected.M.Value, actual.M.Value);
            else
                Assert.IsTrue(actual.M.IsNull);
        }



        private void ReadLineStringTest(string input, int? expectedReference, params PositionData[] expected)
        {
            SqlGeography p;
            if (expectedReference.HasValue)
                p = SqlGeography.STGeomFromText(new System.Data.SqlTypes.SqlChars(input), expectedReference.Value);
            else
                p = SqlGeography.Parse(new System.Data.SqlTypes.SqlString(input));
            Assert.IsNotNull(p);
            VerifyAsLineString(p, expected);

            if (expectedReference != null)
            {
                Assert.AreEqual(p.STSrid, expectedReference);
            }
        }

        private void ReadGeographyPolygonTest(string input, int? expectedReference, params PositionData[][] expected)
        {
            SqlGeography p;
            if (expectedReference.HasValue)
                p = SqlGeography.STGeomFromText(new System.Data.SqlTypes.SqlChars(input), expectedReference.Value);
            else
                p = SqlGeography.Parse(new System.Data.SqlTypes.SqlString(input));
            Assert.IsNotNull(p);
            VerifyAsPolygon(p, expected);

            if (expectedReference != null)
            {
                Assert.AreEqual(p.STSrid, expectedReference);
            }
            else
            {
                Assert.AreEqual(p.STSrid, 4326);
            }
        }

        private void ReadGeometryPolygonTest(string input, int? expectedReference, params PositionData[][] expected)
        {
            SqlGeometry p;
            if (expectedReference.HasValue)
                p = SqlGeometry.STGeomFromText(new System.Data.SqlTypes.SqlChars(input), expectedReference.Value);
            else
                p = SqlGeometry.Parse(new System.Data.SqlTypes.SqlString(input));
            Assert.IsNotNull(p);
            VerifyAsPolygon(p, expected);

            if (expectedReference != null)
            {
                Assert.AreEqual(p.STSrid, expectedReference);
            }
            else
            {
                Assert.AreEqual(p.STSrid, 0);
            }
        }

        [TestMethod]
        [TestCategory("SqlGeography")]
        public void ReaderIgnoreWhitespace()
        {
            ReadGeographyPointTest("POINT\t( 10 \r\n 20 )", new PositionData(20, 10), 4326);
        }

        [TestMethod]
        [TestCategory("SqlGeography")]
        public void ReaderIgnoreCase()
        {
            ReadGeographyPointTest("pOinT( 10 20 )", new PositionData(20, 10), 4326);
        }

        [TestMethod]
        [TestCategory("SqlGeography")]
        public void ReadGeographyPoint()
        {
            ReadGeographyPointTest("POINT EMPTY", null);
            ReadGeographyPointTest("POINT (10 20)", new PositionData(20, 10));
            ReadGeographyPointTest("POINT (10.1 20)", new PositionData(20, 10.1));
            ReadGeographyPointTest("POINT (10.1 20.1)", new PositionData(20.1, 10.1));
            ReadGeographyPointTest("POINT (10 20.1)", new PositionData(20.1, 10));
            ReadGeographyPointTest("POINT (-10 20)", new PositionData(20, -10));
            ReadGeographyPointTest("POINT (+10.1 20)", new PositionData(20, 10.1));
            ReadGeographyPointTest("POINT (10.1 -20.1)", new PositionData(-20.1, 10.1));
            ReadGeographyPointTest("POINT (10 +20.1)", new PositionData(+20.1, 10));
            ReadGeographyPointTest("POINT (10 20 30)", new PositionData(20, 10, 30, null));
            ReadGeographyPointTest("POINT (10 20.1 30)", new PositionData(20.1, 10, 30, null));
            ReadGeographyPointTest("POINT (10 20 30.1)", new PositionData(20, 10, 30.1, null));
            ReadGeographyPointTest("POINT (10 20 -30.0)", new PositionData(20, 10, -30, null));
            ReadGeographyPointTest("POINT (10 20.1 +30.1)", new PositionData(20.1, 10, 30.1, null));
            ReadGeographyPointTest("POINT (10 20 NULL)", new PositionData(20, 10, null, null));
            ReadGeographyPointTest("POINT (10 20 30 40)", new PositionData(20, 10, 30, 40));
            ReadGeographyPointTest("POINT (10 20 30.1 40)", new PositionData(20, 10, 30.1, 40));
            ReadGeographyPointTest("POINT (10 20 30 40.1)", new PositionData(20, 10, 30, 40.1));
            ReadGeographyPointTest("POINT (10 20 30 -40)", new PositionData(20, 10, 30, -40));
            ReadGeographyPointTest("POINT (10 20 30.1 +40.5)", new PositionData(20, 10, 30.1, 40.5));
            ReadGeographyPointTest("POINT (10 20 NULL 40)", new PositionData(20, 10, null, 40));
            ReadGeographyPointTest("POINT (10 20 30.1 NULL)", new PositionData(20, 10, 30.1, null));
            ReadGeographyPointTest("POINT (10 20 NULL NULL)", new PositionData(20, 10, null, null));
        }

        [TestMethod]
        [TestCategory("SqlGeometry")]
        public void ReadGeometryPoint()
        {
            ReadGeometryPointTest("POINT EMPTY", null);
            ReadGeometryPointTest("POINT (10 20)", new PositionData(10, 20));
            ReadGeometryPointTest("POINT (10.1 20)", new PositionData(10.1, 20));
            ReadGeometryPointTest("POINT (10.1 20.1)", new PositionData(10.1, 20.1));
            ReadGeometryPointTest("POINT (10 20.1)", new PositionData(10, 20.1));
            ReadGeometryPointTest("POINT (-10 20)", new PositionData(-10, 20));
            ReadGeometryPointTest("POINT (+10.1 20)", new PositionData(10.1, 20));
            ReadGeometryPointTest("POINT (10.1 -20.1)", new PositionData(10.1, -20.1));
            ReadGeometryPointTest("POINT (10 +20.1)", new PositionData(10, +20.1));
            ReadGeometryPointTest("POINT (10 20 30)", new PositionData(10, 20, 30, null));
            ReadGeometryPointTest("POINT (10 20.1 30)", new PositionData(10, 20.1, 30, null));
            ReadGeometryPointTest("POINT (10 20 30.1)", new PositionData(10, 20, 30.1, null));
            ReadGeometryPointTest("POINT (10 20 -30.0)", new PositionData(10, 20, -30, null));
            ReadGeometryPointTest("POINT (10 20.1 +30.1)", new PositionData(10, 20.1, 30.1, null));
            ReadGeometryPointTest("POINT (10 20 NULL)", new PositionData(10, 20, null, null));
            ReadGeometryPointTest("POINT (10 20 30 40)", new PositionData(10, 20, 30, 40));
            ReadGeometryPointTest("POINT (10 20 30.1 40)", new PositionData(10, 20, 30.1, 40));
            ReadGeometryPointTest("POINT (10 20 30 40.1)", new PositionData(10, 20, 30, 40.1));
            ReadGeometryPointTest("POINT (10 20 30 -40)", new PositionData(10, 20, 30, -40));
            ReadGeometryPointTest("POINT (10 20 30.1 +40.5)", new PositionData(10, 20, 30.1, 40.5));
            ReadGeometryPointTest("POINT (10 20 NULL 40)", new PositionData(10, 20, null, 40));
            ReadGeometryPointTest("POINT (10 20 30.1 NULL)", new PositionData(10, 20, 30.1, null));
            ReadGeometryPointTest("POINT (10 20 NULL NULL)", new PositionData(10, 20, null, null));
        }

        [TestMethod]
        public void ReadLineString()
        {
            this.ReadLineStringTest("LINESTRING EMPTY", null, null);
            this.ReadLineStringTest("LINESTRING (10 20, 10.1 20.1)", null, new PositionData(20, 10, null, null), new PositionData(20.1, 10.1, null, null));
            this.ReadLineStringTest("LINESTRING (10 20 30 40, 40 30 20 10)", null, new PositionData(20, 10, 30, 40), new PositionData(30, 40, 20, 10));
            this.ReadLineStringTest("LINESTRING (10 20 30 40, 40 30 NULL NULL, 30 20 NULL 10)", null, new PositionData(20, 10, 30, 40), new PositionData(30, 40, null, null), new PositionData(20, 30, null, 10));
        }

        [TestMethod]
        [TestCategory("SqlGeography")]
        public void ReadGeographyPolygonEmpty()
        {
            this.ReadGeographyPolygonTest("POLYGON EMPTY", null, null);
        }

        [TestMethod]
        [TestCategory("SqlGeography")]
        public void ReadInvalidGeographyPolygon()
        {
            AssertFormatException(() => SqlGeography.Parse("POLYGON ((10 20, 15 25, 20 30, 10 20), (15 25, 20 30, 25 35, 15 25), EMPTY, (5 5, 6 6, 7 7, 5 5))"),
                "24305: The Polygon input is not valid because the ring number 3 does not have enough points. Each ring of a polygon must contain at least four points.");
        }

        [TestMethod]
        [TestCategory("SqlGeometry")]
        public void ReadGeometryPolygonEmpty()
        {
            this.ReadGeometryPolygonTest("POLYGON EMPTY", null, null);
        }

        [TestMethod]
        [TestCategory("SqlGeometry")]
        public void ReadGeometryPolygonInvalid()
        {
            AssertFormatException(() => SqlGeography.Parse("POLYGON ((10 20, 15 25, 20 30, 10 20), (15 25, 20 30, 25 35, 15 25), EMPTY, (5 5, 6 6, 7 7, 5 5))"),
                "24305: The Polygon input is not valid because the ring number 3 does not have enough points. Each ring of a polygon must contain at least four points.");

        }

        [TestMethod]
        [TestCategory("SqlGeography")]
        public void ReadGeographyMultiPoint_Empty()
        {
            var p = SqlGeography.Parse("MULTIPOINT EMPTY");
            Assert.IsNotNull(p);
            VerifyAsMultiPoint(p, null);
        }

        [TestMethod]
        [TestCategory("SqlGeometry")]
        public void ReadGeometryMultiPoint_Empty()
        {
            var p = SqlGeometry.Parse("MULTIPOINT EMPTY");
            Assert.IsNotNull(p);
            VerifyAsMultiPoint(p, null);
        }

        [TestMethod]
        [TestCategory("SqlGeography")]
        public void ReadGeographyMultiPoint()
        {
            var p = SqlGeography.Parse("MULTIPOINT ((10 20), EMPTY, (30 40))");
            Assert.IsNotNull(p);
            VerifyAsMultiPoint(p, new PositionData(20, 10), null, new PositionData(40, 30));
        }

        [TestMethod]
        [TestCategory("SqlGeometry")]
        public void ReadGeometryMultiPoint()
        {
            var p = SqlGeometry.Parse("MULTIPOINT ((10 20), EMPTY, (30 40))");
            Assert.IsNotNull(p);
            VerifyAsMultiPoint(p, new PositionData(10, 20), null, new PositionData(30, 40));
        }

        [TestMethod]
        public void ReadMultiLineString()
        {
            var p = SqlGeography.Parse("MULTILINESTRING ((10 10, 20 20), EMPTY, (30 30, 40 40, 50 50))");
            Assert.IsNotNull(p);
            VerifyAsMultiLineString(p,
                new[] {
                    new PositionData(10, 10),
                    new PositionData(20, 20),
                },
                null,
                new[] {
                    new PositionData(30, 30),
                    new PositionData(40, 40),
                    new PositionData(50, 50),
                });
        }

        [TestMethod]
        public void ReadCollection_Empty()
        {
            var p = SqlGeography.Parse("GEOMETRYCOLLECTION EMPTY");
            Assert.IsNotNull(p);
            Assert.AreEqual(p.STGeometryType().Value, "GeometryCollection");
            Assert.IsTrue(p.STIsEmpty().Value);
        }

        [TestMethod]
        [TestCategory("SqlGeography")]
        public void ReadGeographyCollection()
        {
            var p = SqlGeography.Parse("GEOMETRYCOLLECTION (POINT(10 11), LINESTRING(20 30, 20 40), POLYGON EMPTY, GEOMETRYCOLLECTION(POINT(30 31)))");
            Assert.IsNotNull(p);
            VerifyAsCollection(p,
                (g) => VerifyAsPoint(g, new PositionData(11, 10)),
                (g) => VerifyAsLineString(g, new PositionData(30, 20), new PositionData(40, 20)),
                (g) => VerifyAsPolygon(g, null),
                (g) => VerifyAsCollection(g, (g1) => VerifyAsPoint(g1, new PositionData(31, 30))));
        }

        [TestMethod]
        [TestCategory("SqlGeometry")]
        public void ReadGeometryCollection()
        {
            var p = SqlGeometry.Parse("GEOMETRYCOLLECTION (POINT(10 11), LINESTRING(20 30, 20 40), POLYGON EMPTY, GEOMETRYCOLLECTION(POINT(30 31)))");
            Assert.IsNotNull(p);
            VerifyAsCollection(p,
                (g) => VerifyAsPoint(g, new PositionData(10, 11)),
                (g) => VerifyAsLineString(g, new PositionData(20, 30), new PositionData(20, 40)),
                (g) => VerifyAsPolygon(g, null),
                (g) => VerifyAsCollection(g, (g1) => VerifyAsPoint(g1, new PositionData(30, 31))));
        }

        [DataRow("POINT(10 20 30)", null)]
        [DataRow("POINT(10 20 30)", null)]
        [DataRow("LINESTRING (10 20 30, 10.1 20.1)", null)]
        [DataRow("LINESTRING (10 20, 10.1 20.1 30.1)", null)]
        [DataRow("POLYGON ((10 20 30, 15 25, 20 30, 10 20), (15 25, 20 30, 25 35, 15 25), EMPTY, (5 5, 6 6, 7 7, 5 5))", "24305: The Polygon input is not valid because the ring number 3 does not have enough points. Each ring of a polygon must contain at least four points.")]
        [DataRow("POLYGON ((10 20 30, 15 25, 20 30, 10 20), (15 25, 20 30 40, 25 35, 15 25), EMPTY, (5 5, 6 6, 7 7, 5 5))", "24305: The Polygon input is not valid because the ring number 3 does not have enough points. Each ring of a polygon must contain at least four points.")]
        [DataRow("POLYGON ((10 20, 15 25, 20 30, 10 20), (15 25, 20 30, 25 35, 15 25), EMPTY, (5 5, 6 6, 7 7 7, 5 5))", "24305: The Polygon input is not valid because the ring number 3 does not have enough points. Each ring of a polygon must contain at least four points.")]
        [DataRow("MULTIPOINT ((10 20 30), EMPTY, (30 40))", null)]
        [DataRow("MULTIPOINT ((10 20), EMPTY, (30 40 50))", null)]
        [DataRow("MULTILINESTRING ((10 10 10, 20 20), EMPTY, (30 30, 40 40, 50 50))", null)]
        [DataRow("MULTILINESTRING ((10 10, 20 20 20), EMPTY, (30 30, 40 40, 50 50))", null)]
        [DataRow("MULTILINESTRING ((10 10, 20 20), EMPTY, (30 30 30, 40 40, 50 50))", null)]
        [DataRow("MULTILINESTRING ((10 10, 20 20), EMPTY, (30 30, 40 40 40, 50 50))", null)]
        [DataRow("MULTILINESTRING ((10 10, 20 20), EMPTY, (30 30, 40 40, 50 50 50))", null)]
        [DataRow("MULTIPOLYGON (((10 10 10, 20 20, 30 30, 10 10), (30 30, 40 40, 50 50, 30 30)), EMPTY, ((10 10, 20 20, 30 30, 10 10)))", null)]
        [DataRow("MULTIPOLYGON (((10 10, 20 20 20, 30 30, 10 10), (30 30, 40 40, 50 50, 30 30)), EMPTY, ((10 10, 20 20, 30 30, 10 10)))", null)]
        [DataRow("MULTIPOLYGON (((10 10, 20 20, 30 30 30, 10 10), (30 30, 40 40, 50 50, 30 30)), EMPTY, ((10 10, 20 20, 30 30, 10 10)))", null)]
        [DataRow("MULTIPOLYGON (((10 10, 20 20, 30 30, 10 10 10), (30 30, 40 40, 50 50, 30 30)), EMPTY, ((10 10, 20 20, 30 30, 10 10)))", null)]
        [DataRow("MULTIPOLYGON (((10 10, 20 20, 30 30, 10 10), (30 30 30, 40 40, 50 50, 30 30)), EMPTY, ((10 10, 20 20, 30 30, 10 10)))", null)]
        [DataRow("MULTIPOLYGON (((10 10, 20 20, 30 30, 10 10), (30 30, 40 40 40, 50 50, 30 30)), EMPTY, ((10 10, 20 20, 30 30, 10 10)))", null)]
        [DataRow("MULTIPOLYGON (((10 10, 20 20, 30 30, 10 10), (30 30, 40 40, 50 50 50, 30 30)), EMPTY, ((10 10, 20 20, 30 30, 10 10)))", null)]
        [DataRow("MULTIPOLYGON (((10 10, 20 20, 30 30, 10 10), (30 30, 40 40, 50 50, 30 30 30)), EMPTY, ((10 10, 20 20, 30 30, 10 10)))", null)]
        [DataRow("MULTIPOLYGON (((10 10, 20 20, 30 30, 10 10), (30 30, 40 40, 50 50, 30 30)), EMPTY, ((10 10 10, 20 20, 30 30, 10 10)))", null)]
        [DataRow("MULTIPOLYGON (((10 10, 20 20, 30 30, 10 10), (30 30, 40 40, 50 50, 30 30)), EMPTY, ((10 10, 20 20 20, 30 30, 10 10)))", null)]
        [DataRow("MULTIPOLYGON (((10 10, 20 20, 30 30, 10 10), (30 30, 40 40, 50 50, 30 30)), EMPTY, ((10 10, 20 20, 30 30 30, 10 10)))", null)]
        [DataRow("MULTIPOLYGON (((10 10, 20 20, 30 30, 10 10), (30 30, 40 40, 50 50, 30 30)), EMPTY, ((10 10, 20 20, 30 30, 10 10 10)))", null)]
        [DataRow("GEOMETRYCOLLECTION (POINT(10 10 10), LINESTRING(20 20, 20 20), POLYGON EMPTY, GEOMETRYCOLLECTION(POINT(30 30)))", null)]
        [DataRow("GEOMETRYCOLLECTION (POINT(10 10), LINESTRING(20 20 20, 20 20), POLYGON EMPTY, GEOMETRYCOLLECTION(POINT(30 30)))", null)]
        [DataRow("GEOMETRYCOLLECTION (POINT(10 10), LINESTRING(20 20, 20 20 20), POLYGON EMPTY, GEOMETRYCOLLECTION(POINT(30 30)))", null)]
        [DataRow("GEOMETRYCOLLECTION (POINT(10 10), LINESTRING(20 20, 20 20), POLYGON EMPTY, GEOMETRYCOLLECTION(POINT(30 30 30)))", null)]
        [TestMethod]
        public void ErrorOnRead3DPoint(string wktValue, string error)
        {
            try
            {
                SqlGeography.Parse(new System.Data.SqlTypes.SqlString(wktValue));
                if (!string.IsNullOrEmpty(error))
                    Assert.Fail("Exception expected");
            }
            catch (System.FormatException ex)
            {
                if (!string.IsNullOrEmpty(error))
                    Assert.IsTrue(ex.Message.StartsWith(error), ex.Message);
            }
        }

        [TestMethod]
        public void ReadUnknownTagTest()
        {
            AssertFormatException(() => SqlGeography.Parse("SRID=1234;FOO(10 20)"), ""); // really should be: 24114: The label SRID=1234;FOO(10 20) in the input well-known text (WKT) is not valid. Valid labels are POINT, LINESTRING, POLYGON, MULTIPOINT, MULTILINESTRING, MULTIPOLYGON, GEOMETRYCOLLECTION, CIRCULARSTRING, COMPOUNDCURVE, CURVEPOLYGON and FULLGLOBE (geography Data Type only).
        }


        [TestMethod]
        public void ReadUnexpectedCharacter()
        {
            AssertFormatException(() => SqlGeography.Parse("POINT:10 20"), ""); // Really should be "24142: Expected \"(\" at position 5. The input has \":\"."
        }

        [TestMethod]
        public void ReadEmptyString()
        {
            AssertFormatException(() => SqlGeography.Parse(""), "24112: The well-known text (WKT) input is empty. To input an empty instance, specify an empty instance of one of the following types: Point, LineString, Polygon, MultiPoint, MultiLineString, MultiPolygon, CircularString, CompoundCurve, CurvePolygon or GeometryCollection.");
        }

        [TestMethod]
        public void ReadUnexpectedToken()
        {
            AssertFormatException(() => SqlGeography.Parse("POINT(10,20)"), ""); //Really should be "24141: A number is expected at position 11 of the input. The input has ,20."
        }
        private static void AssertFormatException(Action action, string message)
        {
            try
            {
                action();
                Assert.Fail("Exception expected");
            }
            catch (FormatException ex)
            {
                if (message != String.Empty)
                    Assert.AreEqual(message, ex.Message);
            }
        }

        [TestMethod]
        public void OneParserMultipleStreamsOfGeographies()
        {
            ReadGeographyPointTest("POINT EMPTY", null);
            ReadGeographyPointTest("POINT (10 20)", new PositionData(20, 10));
            ReadGeographyPointTest("POINT (10.1 20)", new PositionData(20, 10.1));
            ReadGeographyPointTest("POINT EMPTY", null);
            ReadGeographyPointTest("POINT (10.1 20.1)", new PositionData(20.1, 10.1));
            ReadGeographyPointTest("POINT (10 20.1)", new PositionData(20.1, 10));
        }

    }
}
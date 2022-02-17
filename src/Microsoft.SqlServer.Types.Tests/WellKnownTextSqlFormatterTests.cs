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
    public class WellKnownTextWriterTests
    {
        [TestMethod]
        public void WritePoint()
        {
            Action<SqlGeographyBuilder> emptyCalls = (w) =>
            {
                w.BeginGeography(OpenGisGeographyType.Point);
                w.EndGeography();
            };
            GeographyToWktTest(true, emptyCalls, "POINT EMPTY");
            GeographyToWktTest(false, emptyCalls, "POINT EMPTY");

            Action<SqlGeographyBuilder> d4PointCalls = (w) =>
            {
                w.BeginGeography(OpenGisGeographyType.Point);
                w.BeginFigure(10, 20, 30, 40);
                w.EndFigure();
                w.EndGeography();
            };
            GeographyToWktTest(true, d4PointCalls, "POINT (20 10)");
            GeographyToWktTest(false, d4PointCalls, "POINT (20 10 30 40)");

            Action<SqlGeographyBuilder> d3PointCalls = (w) =>
            {
                w.BeginGeography(OpenGisGeographyType.Point);
                w.BeginFigure(10, 20, 30, null);
                w.EndFigure();
                w.EndGeography();
            };
            GeographyToWktTest(true, d3PointCalls, "POINT (20 10)");
            GeographyToWktTest(false, d3PointCalls, "POINT (20 10 30)");

            Action<SqlGeographyBuilder> d2PointCalls = (w) =>
            {
                w.BeginGeography(OpenGisGeographyType.Point);
                w.BeginFigure(10, 20, null, null);
                w.EndFigure();
                w.EndGeography();
            };
            GeographyToWktTest(true, d2PointCalls, "POINT (20 10)");
            GeographyToWktTest(false, d2PointCalls, "POINT (20 10)");

            Action<SqlGeographyBuilder> skipPointCalls =
            (w) =>
            {
                w.BeginGeography(OpenGisGeographyType.Point);
                w.BeginFigure(10, 20, null, 40);
                w.EndFigure();
                w.EndGeography();
            };
            GeographyToWktTest(true, skipPointCalls, "POINT (20 10)");
            GeographyToWktTest(false, skipPointCalls, "POINT (20 10 NULL 40)");
        }

        [TestMethod]
        public void WriteLineString()
        {

            Action<SqlGeographyBuilder> emptyCalls = (w) =>
            {
                w.BeginGeography(OpenGisGeographyType.LineString);
                w.EndGeography();
            };

            GeographyToWktTest(true, emptyCalls, "LINESTRING EMPTY");
            GeographyToWktTest(false, emptyCalls, "LINESTRING EMPTY");

            Action<SqlGeographyBuilder> twoD2Point = (w) =>
            {
                w.BeginGeography(OpenGisGeographyType.LineString);
                w.BeginFigure(10, 20, null, null);
                w.AddLine(20, 30, null, null);
                w.EndFigure();
                w.EndGeography();
            };

            GeographyToWktTest(true, twoD2Point, "LINESTRING (20 10, 30 20)");
            GeographyToWktTest(false, twoD2Point, "LINESTRING (20 10, 30 20)");

            Action<SqlGeographyBuilder> threeD2Point = (w) =>
            {
                w.BeginGeography(OpenGisGeographyType.LineString);
                w.BeginFigure(10, 20, null, null);
                w.AddLine(-20.5, -30, null, null);
                w.AddLine(30, 40, null, null);
                w.EndFigure();
                w.EndGeography();
            };
            GeographyToWktTest(true, threeD2Point, "LINESTRING (20 10, -30 -20.5, 40 30)");
            GeographyToWktTest(false, threeD2Point, "LINESTRING (20 10, -30 -20.5, 40 30)");


            Action<SqlGeographyBuilder> twoD4Point = (w) =>
            {
                w.BeginGeography(OpenGisGeographyType.LineString);
                w.BeginFigure(10, 20, 30, 40);
                w.AddLine(20, 30, 40, 50);
                w.EndFigure();
                w.EndGeography();
            };

            GeographyToWktTest(true, twoD4Point, "LINESTRING (20 10, 30 20)");
            GeographyToWktTest(false, twoD4Point, "LINESTRING (20 10 30 40, 30 20 40 50)");
        }

        [TestMethod]
        public void WritePolygon()
        {
            Action<SqlGeographyBuilder> emptyCalls = (w) =>
            {
                w.BeginGeography(OpenGisGeographyType.Polygon);
                w.EndGeography();
            };
            GeographyToWktTest(true, emptyCalls, "POLYGON EMPTY");
            GeographyToWktTest(false, emptyCalls, "POLYGON EMPTY");


            Action<SqlGeographyBuilder> fourD2Point = (w) =>
            {
                w.BeginGeography(OpenGisGeographyType.Polygon);
                w.BeginFigure(10, 20, null, null);
                w.AddLine(20, 30, null, null);
                w.AddLine(30, 40, null, null);
                w.AddLine(10, 20, null, null);
                w.EndFigure();
                w.EndGeography();
            };
            GeographyToWktTest(true, fourD2Point, "POLYGON ((20 10, 30 20, 40 30, 20 10))");
            GeographyToWktTest(false, fourD2Point, "POLYGON ((20 10, 30 20, 40 30, 20 10))");

            Action<SqlGeographyBuilder> fourD2PointWith2D2Holes = (w) =>
            {
                w.BeginGeography(OpenGisGeographyType.Polygon);
                w.BeginFigure(10, 20, null, null);
                w.AddLine(20, 30, null, null);
                w.AddLine(30, 40, null, null);
                w.AddLine(10, 20, null, null);
                w.EndFigure();

                w.BeginFigure(-10, -20, null, null);
                w.AddLine(-20, -30, null, null);
                w.AddLine(-30, -40, null, null);
                w.AddLine(-10, -20, null, null);
                w.EndFigure();

                w.BeginFigure(-10.5, -20.5, null, null);
                w.AddLine(-20.5, -30.5, null, null);
                w.AddLine(-30.5, -40.5, null, null);
                w.AddLine(-10.5, -20.5, null, null);
                w.EndFigure();
                w.EndGeography();
            };

            GeographyToWktTest(true, fourD2PointWith2D2Holes, "POLYGON ((20 10, 30 20, 40 30, 20 10), (-20 -10, -30 -20, -40 -30, -20 -10), (-20.5 -10.5, -30.5 -20.5, -40.5 -30.5, -20.5 -10.5))");
            GeographyToWktTest(false, fourD2PointWith2D2Holes, "POLYGON ((20 10, 30 20, 40 30, 20 10), (-20 -10, -30 -20, -40 -30, -20 -10), (-20.5 -10.5, -30.5 -20.5, -40.5 -30.5, -20.5 -10.5))");
        }

        [TestMethod]
        public void WriteMultiPoint()
        {
            Action<SqlGeographyBuilder> twoEmptyPointsCalls = (w) =>
            {
                w.BeginGeography(OpenGisGeographyType.MultiPoint);
                w.BeginGeography(OpenGisGeographyType.Point);
                w.EndGeography();
                w.BeginGeography(OpenGisGeographyType.Point);
                w.EndGeography();
                w.EndGeography();
            };
            GeographyToWktTest(true, twoEmptyPointsCalls, "MULTIPOINT (EMPTY, EMPTY)");
            GeographyToWktTest(false, twoEmptyPointsCalls, "MULTIPOINT (EMPTY, EMPTY)");

            Action<SqlGeographyBuilder> noPointsCalls = (w) =>
            {
                w.BeginGeography(OpenGisGeographyType.MultiPoint);
                w.EndGeography();
            };
            GeographyToWktTest(true, noPointsCalls, "MULTIPOINT EMPTY");
            GeographyToWktTest(false, noPointsCalls, "MULTIPOINT EMPTY");

            Action<SqlGeographyBuilder> twoD2PointsCalls = (w) =>
            {
                w.BeginGeography(OpenGisGeographyType.MultiPoint);
                w.BeginGeography(OpenGisGeographyType.Point);
                w.BeginFigure(10, 20, null, null);
                w.EndFigure();
                w.EndGeography();

                w.BeginGeography(OpenGisGeographyType.Point);
                w.EndGeography();

                w.BeginGeography(OpenGisGeographyType.Point);
                w.BeginFigure(30, 40, null, null);
                w.EndFigure();
                w.EndGeography();
                w.EndGeography();
            };
            GeographyToWktTest(true, twoD2PointsCalls, "MULTIPOINT ((20 10), EMPTY, (40 30))");
            GeographyToWktTest(false, twoD2PointsCalls, "MULTIPOINT ((20 10), EMPTY, (40 30))");

            Action<SqlGeographyBuilder> singleD3PointCalls = (w) =>
            {
                w.BeginGeography(OpenGisGeographyType.MultiPoint);
                w.BeginGeography(OpenGisGeographyType.Point);
                w.BeginFigure(10, 20, 30, 40);
                w.EndFigure();
                w.EndGeography();
                w.EndGeography();
            };
            GeographyToWktTest(true, singleD3PointCalls, "MULTIPOINT ((20 10))");
            GeographyToWktTest(false, singleD3PointCalls, "MULTIPOINT ((20 10 30 40))");
        }

        [TestMethod]
        public void WriteMultiLineString()
        {
            Action<SqlGeographyBuilder> emptyCalls = (w) =>
            {
                w.BeginGeography(OpenGisGeographyType.MultiLineString);
                w.BeginGeography(OpenGisGeographyType.LineString);
                w.EndGeography();
                w.EndGeography();
            };
            GeographyToWktTest(true, emptyCalls, "MULTILINESTRING (EMPTY)");
            GeographyToWktTest(false, emptyCalls, "MULTILINESTRING (EMPTY)");

            Action<SqlGeographyBuilder> emptyCalls2 = (w) =>
            {
                w.BeginGeography(OpenGisGeographyType.MultiLineString);
                w.EndGeography();
            };
            GeographyToWktTest(true, emptyCalls2, "MULTILINESTRING EMPTY");
            GeographyToWktTest(false, emptyCalls2, "MULTILINESTRING EMPTY");

            Action<SqlGeographyBuilder> twoD2LineStringCalls = (w) =>
            {
                w.BeginGeography(OpenGisGeographyType.MultiLineString);
                w.BeginGeography(OpenGisGeographyType.LineString);
                w.BeginFigure(10, 20, null, null);
                w.AddLine(20, 30, null, null);
                w.EndFigure();
                w.EndGeography();

                w.BeginGeography(OpenGisGeographyType.LineString);
                w.EndGeography();

                w.BeginGeography(OpenGisGeographyType.LineString);
                w.BeginFigure(30, 40, null, null);
                w.AddLine(40, 50, null, null);
                w.EndFigure();
                w.EndGeography();
                w.EndGeography();
            };
            GeographyToWktTest(true, twoD2LineStringCalls, "MULTILINESTRING ((20 10, 30 20), EMPTY, (40 30, 50 40))");
            GeographyToWktTest(false, twoD2LineStringCalls, "MULTILINESTRING ((20 10, 30 20), EMPTY, (40 30, 50 40))");

            Action<SqlGeographyBuilder> singleD3LineStringCalls = (w) =>
            {
                w.BeginGeography(OpenGisGeographyType.MultiLineString);
                w.BeginGeography(OpenGisGeographyType.LineString);
                w.BeginFigure(10, 20, 40, null);
                w.AddLine(20, 30, 50, null);
                w.EndFigure();
                w.EndGeography();
                w.EndGeography();
            };
            GeographyToWktTest(true, singleD3LineStringCalls, "MULTILINESTRING ((20 10, 30 20))");
            GeographyToWktTest(false, singleD3LineStringCalls, "MULTILINESTRING ((20 10 40, 30 20 50))");
        }

        [TestMethod]
        public void WriteMultiPolygon()
        {
            Action<SqlGeographyBuilder> emptyCalls = (w) =>
            {
                w.BeginGeography(OpenGisGeographyType.MultiPolygon);
                w.BeginGeography(OpenGisGeographyType.Polygon);
                w.EndGeography();
                w.EndGeography();
            };
            GeographyToWktTest(true, emptyCalls, "MULTIPOLYGON (EMPTY)");
            GeographyToWktTest(false, emptyCalls, "MULTIPOLYGON (EMPTY)");

            Action<SqlGeographyBuilder> emptyCalls2 = (w) =>
            {
                w.BeginGeography(OpenGisGeographyType.MultiPolygon);
                w.EndGeography();
            };
            GeographyToWktTest(true, emptyCalls2, "MULTIPOLYGON EMPTY");
            GeographyToWktTest(false, emptyCalls2, "MULTIPOLYGON EMPTY");

            Action<SqlGeographyBuilder> threeLineD2Calls = (w) =>
            {
                w.BeginGeography(OpenGisGeographyType.MultiPolygon);
                w.BeginGeography(OpenGisGeographyType.Polygon);
                w.BeginFigure(10, 20, null, null);
                w.AddLine(20, 30, null, null);
                w.AddLine(30, 40, null, null);
                w.AddLine(10, 20, null, null);
                w.EndFigure();

                w.BeginFigure(-10.5, -20.5, null, null);
                w.AddLine(-20.5, -30.5, null, null);
                w.AddLine(-30.5, -40.5, null, null);
                w.AddLine(-10.5, -20.5, null, null);
                w.EndFigure();
                w.EndGeography();

                w.BeginGeography(OpenGisGeographyType.Polygon);
                w.EndGeography();

                w.BeginGeography(OpenGisGeographyType.Polygon);

                w.BeginFigure(10, 20, null, null);
                w.AddLine(20, 30, null, null);
                w.AddLine(30, 40, null, null);
                w.AddLine(10, 20, null, null);
                w.EndFigure();

                w.EndGeography();
                w.EndGeography();
            };
            GeographyToWktTest(true, threeLineD2Calls, "MULTIPOLYGON (((20 10, 30 20, 40 30, 20 10), (-20.5 -10.5, -30.5 -20.5, -40.5 -30.5, -20.5 -10.5)), EMPTY, ((20 10, 30 20, 40 30, 20 10)))");
            GeographyToWktTest(false, threeLineD2Calls, "MULTIPOLYGON (((20 10, 30 20, 40 30, 20 10), (-20.5 -10.5, -30.5 -20.5, -40.5 -30.5, -20.5 -10.5)), EMPTY, ((20 10, 30 20, 40 30, 20 10)))");
        }

        [TestMethod]
        public void WriteCollection()
        {
            Action<SqlGeographyBuilder> emptyCalls = (w) =>
            {
                w.BeginGeography(OpenGisGeographyType.GeometryCollection);
                w.EndGeography();
            };
            GeographyToWktTest(true, emptyCalls, "GEOMETRYCOLLECTION EMPTY");
            GeographyToWktTest(false, emptyCalls, "GEOMETRYCOLLECTION EMPTY");

            Action<SqlGeographyBuilder> emptyCalls2 = (w) =>
            {
                w.BeginGeography(OpenGisGeographyType.GeometryCollection);
                w.BeginGeography(OpenGisGeographyType.Point);
                w.EndGeography();
                w.BeginGeography(OpenGisGeographyType.LineString);
                w.EndGeography();
                w.EndGeography();
            };
            GeographyToWktTest(true, emptyCalls2, "GEOMETRYCOLLECTION (POINT EMPTY, LINESTRING EMPTY)");
            GeographyToWktTest(false, emptyCalls2, "GEOMETRYCOLLECTION (POINT EMPTY, LINESTRING EMPTY)");

            Action<SqlGeographyBuilder> nestedEmptyCalls = (w) =>
            {
                w.BeginGeography(OpenGisGeographyType.GeometryCollection);
                w.BeginGeography(OpenGisGeographyType.GeometryCollection);
                w.EndGeography();
                w.EndGeography();
            };
            GeographyToWktTest(true, nestedEmptyCalls, "GEOMETRYCOLLECTION (GEOMETRYCOLLECTION EMPTY)");
            GeographyToWktTest(false, nestedEmptyCalls, "GEOMETRYCOLLECTION (GEOMETRYCOLLECTION EMPTY)");

            Action<SqlGeographyBuilder> singlePointCalls = (w) =>
            {
                w.BeginGeography(OpenGisGeographyType.GeometryCollection);
                w.BeginGeography(OpenGisGeographyType.Point);
                w.BeginFigure(10, 20, 30, 40);
                w.EndFigure();
                w.EndGeography();
                w.EndGeography();
            };
            GeographyToWktTest(true, singlePointCalls, "GEOMETRYCOLLECTION (POINT (20 10))");
            GeographyToWktTest(false, singlePointCalls, "GEOMETRYCOLLECTION (POINT (20 10 30 40))");

            Action<SqlGeographyBuilder> pointMultiPointCalls = (w) =>
            {
                w.BeginGeography(OpenGisGeographyType.GeometryCollection);
                w.BeginGeography(OpenGisGeographyType.Point);
                w.BeginFigure(10, 20, null, null);
                w.EndFigure();
                w.EndGeography();

                w.BeginGeography(OpenGisGeographyType.MultiPoint);
                w.BeginGeography(OpenGisGeographyType.Point);
                w.BeginFigure(20, 30, null, null);
                w.EndFigure();
                w.EndGeography();
                w.BeginGeography(OpenGisGeographyType.Point);
                w.BeginFigure(30, 40, null, null);
                w.EndFigure();
                w.EndGeography();
                w.EndGeography();

                w.EndGeography();
            };
            GeographyToWktTest(true, pointMultiPointCalls, "GEOMETRYCOLLECTION (POINT (20 10), MULTIPOINT ((30 20), (40 30)))");
            GeographyToWktTest(false, pointMultiPointCalls, "GEOMETRYCOLLECTION (POINT (20 10), MULTIPOINT ((30 20), (40 30)))");
        }

        private static void GeographyToWktTest(bool is2d, Action<SqlGeographyBuilder> pipelineAction, string expectedWkt)
        {
            var builder = new SqlGeographyBuilder();
            builder.SetSrid(4326);
            pipelineAction.Invoke(builder);

            if (is2d)
                Assert.AreEqual(expectedWkt, builder.ConstructedGeography.STAsText().ToSqlString());
            else
                Assert.AreEqual(expectedWkt, builder.ConstructedGeography.AsTextZM().ToSqlString());
        }

    }
}
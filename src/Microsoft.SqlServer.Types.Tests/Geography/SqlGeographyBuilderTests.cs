using System;
using Microsoft.SqlServer.Types.Tests.Geometry;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.SqlServer.Types.Tests.Geography
{
    [TestClass]
    [TestCategory("SqlGeography")]
    [TestCategory("Sink")]
    public class SqlGeographyBuilderTests
    {
        [TestMethod]
        public void CreateEmpty()
        {
            SqlGeographyBuilder b = new SqlGeographyBuilder();
            b.SetSrid(4326);
            b.BeginGeography(OpenGisGeographyType.Point);
            b.EndGeography();
            var g = b.ConstructedGeography;
            Assert.IsTrue(g.STIsEmpty().Value);
        }
        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CreateEmptyNoSrid()
        {
            SqlGeographyBuilder b = new SqlGeographyBuilder();
            var g = b.ConstructedGeography;
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void GetConstructedBeforeCompletion()
        {
            SqlGeographyBuilder b = new SqlGeographyBuilder();
            b.SetSrid(4326);
            var g = b.ConstructedGeography;
            Assert.IsTrue(g.STIsEmpty().Value);
        }

        [TestMethod]
        public void BuildPoint()
        {
            SqlGeographyBuilder b = new SqlGeographyBuilder();
            b.SetSrid(4326);
            b.BeginGeography(OpenGisGeographyType.Point);
            b.BeginFigure(1, 2);
            b.EndFigure();
            b.EndGeography();
            var g = b.ConstructedGeography;
            Assert.AreEqual("Point", g.STGeometryType());
            Assert.AreEqual(2, g.Long);
            Assert.AreEqual(1, g.Lat);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void EndGeographyBeforeBeginGeography()
        {
            SqlGeographyBuilder b = new SqlGeographyBuilder();
            b.SetSrid(4326);
            b.EndGeography();
            var g = b.ConstructedGeography;
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void BeginFigureBeforeBeginGeography()
        {
            SqlGeographyBuilder b = new SqlGeographyBuilder();
            b.SetSrid(4326);
            b.BeginFigure(1, 2);
            var g = b.ConstructedGeography;
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void EndFigureBeforeBeginFigure()
        {
            SqlGeographyBuilder b = new SqlGeographyBuilder();
            b.SetSrid(4326);
            b.BeginGeography(OpenGisGeographyType.Point);
            b.EndFigure();
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void BuildPoint_NoSrid()
        {
            SqlGeographyBuilder b = new SqlGeographyBuilder();
            b.BeginGeography(OpenGisGeographyType.Point); // Should throw format exception
        }

        [TestMethod]
        public void BuildLineString()
        {
            SqlGeographyBuilder b = new SqlGeographyBuilder();
            b.SetSrid(4326);
            b.BeginGeography(OpenGisGeographyType.LineString);
            b.BeginFigure(1, 2);
            b.AddLine(3, 4, 10, null);
            b.AddLine(6, 7, 11, 12);
            b.EndFigure();
            b.EndGeography();
            var g = b.ConstructedGeography;
            Assert.AreEqual("LineString", g.STGeometryType());
            Assert.AreEqual(3, g.STNumPoints());
            Assert.AreEqual(1, g.STStartPoint().Lat);
            Assert.AreEqual(2, g.STStartPoint().Long);
            Assert.AreEqual(6, g.STEndPoint().Lat);
            Assert.AreEqual(7, g.STEndPoint().Long);
            Assert.AreEqual(3, g.STPointN(2).Lat);
            Assert.AreEqual(4, g.STPointN(2).Long);
            Assert.IsTrue(g.STPointN(2).HasZ);
            Assert.IsFalse(g.STPointN(2).HasM);
        }
    }
}

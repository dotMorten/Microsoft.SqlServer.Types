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
        public void CreateEmptyNoSrid()
        {
            SqlGeographyBuilder b = new SqlGeographyBuilder();
            AssertEx.ThrowsException(() => { var g = b.ConstructedGeography; }, typeof(FormatException), "24300: Expected a call to SetSrid, but Finish was called.");
        }

        [TestMethod]
        public void GetConstructedBeforeCompletion()
        {
            SqlGeographyBuilder b = new SqlGeographyBuilder();
            b.SetSrid(4326);
            AssertEx.ThrowsException(() => { var g = b.ConstructedGeography; }, typeof(FormatException), "24300: Expected a call to BeginGeography, but Finish was called.");
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
        public void EndGeographyBeforeBeginGeography()
        {
            SqlGeographyBuilder b = new SqlGeographyBuilder();
            b.SetSrid(4326);

            AssertEx.ThrowsException(() => { b.EndGeography(); }, typeof(FormatException), "24300: Expected a call to BeginGeography, but EndGeography was called.");
            //var g = b.ConstructedGeography;
        }

        [TestMethod]
        public void BeginFigureBeforeBeginGeography()
        {
            SqlGeographyBuilder b = new SqlGeographyBuilder();
            b.SetSrid(4326);
            AssertEx.ThrowsException(() => { b.BeginFigure(1, 2); }, typeof(FormatException), "24300: Expected a call to BeginGeography, but BeginFigure was called.");
        }

        [TestMethod]
        public void EndFigureBeforeBeginFigure()
        {
            SqlGeographyBuilder b = new SqlGeographyBuilder();
            b.SetSrid(4326);
            b.BeginGeography(OpenGisGeographyType.Point);
            AssertEx.ThrowsException(() => { b.EndFigure(); }, typeof(FormatException), "24301: Expected a call to BeginFigure or EndGeography, but EndFigure was called.");
        }

        [TestMethod]
        public void AddLineOnPoint()
        {
            SqlGeographyBuilder b = new SqlGeographyBuilder();
            b.SetSrid(4326);
            b.BeginGeography(OpenGisGeographyType.Point);
            b.BeginFigure(1, 2);
            AssertEx.ThrowsException(() => { b.AddLine(1,2); }, typeof(FormatException), "24300: Expected a call to EndFigure, but AddLine was called.");
        }

        [TestMethod]
        public void AddLineBeforeBeginFigure()
        {
            SqlGeographyBuilder b = new SqlGeographyBuilder();
            b.SetSrid(4326);
            b.BeginGeography(OpenGisGeographyType.LineString);
            AssertEx.ThrowsException(() => { b.AddLine(1, 2); }, typeof(FormatException), "24301: Expected a call to BeginFigure or EndGeography, but AddLine was called.");
        }

        [TestMethod]
        public void BuildPoint_NoSrid()
        {
            SqlGeographyBuilder b = new SqlGeographyBuilder();
            AssertEx.ThrowsException(() => { b.BeginGeography(OpenGisGeographyType.Point); }, typeof(FormatException), "24300: Expected a call to SetSrid, but BeginGeography(Point) was called.");
        }

        [TestMethod]
        public void BeginFigureBeforeBeginGeo()
        {
            SqlGeographyBuilder b = new SqlGeographyBuilder();
            AssertEx.ThrowsException(() => { b.BeginFigure(1, 2); }, typeof(FormatException), "24300: Expected a call to SetSrid, but BeginFigure was called.");
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

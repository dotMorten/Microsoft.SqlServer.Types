using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.SqlServer.Types.Tests.Geoometry
{
    [TestClass]
    [TestCategory("SqlGeometry")]
    [TestCategory("STLength")]
    public class STLengthTests
    {
        [TestMethod]
        public void STLength_Null()
        {
            var id = SqlGeometry.Null;
            var l = id.STLength();
            Assert.IsTrue(l.IsNull);
        }

        [TestMethod]
        public void STLength_Point()
        {
            var id = SqlGeometry.Point(1, 2, 1);
            var l = id.STLength();
            Assert.AreEqual(0d, l.Value);
        }

        [TestMethod]
        public void STLength_LineString()
        {
            var id = SqlGeometry.Parse("LINESTRING (0 0, 10 0, 10 10)");
            var l = id.STLength();
            Assert.AreEqual(20, l.Value);
        }

        [TestMethod]
        public void STLength_MultiLineString()
        {
            var id = SqlGeometry.Parse("MULTILINESTRING ((0 0, 10 0),(20 10, 20 5))");
            var l = id.STLength();
            Assert.AreEqual(15, l.Value);
        }
    }
}

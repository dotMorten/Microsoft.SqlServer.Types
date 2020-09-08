using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.SqlServer.Types.Tests.Geography
{
    [TestClass]
    [TestCategory("SqlGeography")]
    [TestCategory("STLength")]
    public class STLengthTests
    {
        [TestMethod]
        public void STLength_Null()
        {
            var id = SqlGeography.Null;
            var l = id.STLength();
            Assert.IsTrue(l.IsNull);
        }
        
        [TestMethod]
        public void STLength_Point()
        {
            var id = SqlGeography.Point(1, 2, 4326);
            var l = id.STLength();
            Assert.AreEqual(0d, l.Value);
        }

        [TestMethod]
        public void STLength_LineString()
        {
            var id = SqlGeography.Parse("LINESTRING (-117 34, 12 55)");
            var l = id.STLength();
            Assert.AreEqual(9013247.7132437117, l.Value, 9013247.7132437117*0.0000005);
        }

        [TestMethod]
        public void STLength_LineStringAlongEquator()
        {
            var id = SqlGeography.Parse("LINESTRING (-50 0, 0 0, 50 0)");
            var l = id.STLength();
            Assert.AreEqual(11131949.079277, l.Value, 9013247.7132437117 * 0.0000005);
        }

        [TestMethod]
        public void STLength_LineStringOpposite()
        {
            var id = SqlGeography.Parse("LINESTRING (-90 0, 0 90)");
            var l = id.STLength();
            Assert.AreEqual(10001965.6701831, l.Value, 10001965.6701831 * 0.0000005);
        }

        [TestMethod]
        public void STLength_MultiLineString()
        {
            var id = SqlGeography.Parse("MULTILINESTRING ((-117 34, 12 55),(-117 34, 12 55))");
            var l = id.STLength();
            Assert.AreEqual(18026495.4264874234, l.Value, 18026495.4264874234 * 0.0000005);
        }
    }
}

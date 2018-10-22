using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.SqlServer.Types.RefTests
{
    [TestClass]
    [TestCategory("SqlHierarchyId")]
    public class SqlHierarchyIdTests
    {

        [TestMethod]
        public void InsertFirstChild()
        {
            var id = SqlHierarchyId.GetRoot();
            var idChild = id.GetDescendant(SqlHierarchyId.Null, SqlHierarchyId.Null);
            Assert.AreEqual(id, idChild.GetAncestor(1));
        }

        [TestMethod]
        public void GetParentOfRootNode()
        {
            var root = SqlHierarchyId.GetRoot();
            var parent = root.GetAncestor(1);
            Assert.IsTrue(parent.IsNull);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ThrowsOnNegativeAncestor()
        {
            var root = SqlHierarchyId.GetRoot();
            var parent = root.GetAncestor(-1);
        }
    }
}

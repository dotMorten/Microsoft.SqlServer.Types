using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.SqlServer.Types.Tests.HierarchyId
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
        public void RootIsNotNull()
        {
            Assert.AreNotEqual(SqlHierarchyId.GetRoot(), SqlHierarchyId.Null);
        }

        [TestMethod]
        public void RootIsNotNull2()
        {
            Assert.IsFalse(SqlHierarchyId.GetRoot().IsNull);
        }

        [TestMethod]
        public void NullIsNull()
        {
            Assert.IsTrue(SqlHierarchyId.Null.IsNull);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ThrowsOnNegativeAncestor()
        {
            var root = SqlHierarchyId.GetRoot();
            var parent = root.GetAncestor(-1);
        }

        [TestMethod]
        [WorkItem(21)]
        public void GetDescendantsOfChildren()
        {
            var child1 = SqlHierarchyId.Parse("/1/1/");
            var child2 = SqlHierarchyId.Parse("/1/1.1/");
            var newSibling = SqlHierarchyId.Parse("/1/").GetDescendant(child1, child2);
            Assert.AreEqual(newSibling.ToString(), "/1/1.0/");
        }
    }
}

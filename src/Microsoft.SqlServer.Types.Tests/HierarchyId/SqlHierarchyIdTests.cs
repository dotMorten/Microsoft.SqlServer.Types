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
        public void GetDescendantsNormal()
        {
            var newSibling = SqlHierarchyId.Parse("/9/").GetDescendant(
                SqlHierarchyId.Parse("/9/1/"),
                SqlHierarchyId.Parse("/9/3/"));
            Assert.AreEqual("/9/2/", newSibling.ToString(), );
        }

        [TestMethod]
        public void GetDescendantsIncrementFirst()
        {
            var newSibling = SqlHierarchyId.Parse("/9/").GetDescendant(
                SqlHierarchyId.Parse("/9/1.1/"),
                SqlHierarchyId.Parse("/9/2/"));
            Assert.AreEqual("/9/1.2/", newSibling.ToString(), );
        }

        [TestMethod]
        [WorkItem(21)]
        public void GetDescendantsDecrementSecond()
        {
            var newSibling = SqlHierarchyId.Parse("/9/").GetDescendant(
                SqlHierarchyId.Parse("/9/1/"),
                SqlHierarchyId.Parse("/9/1.1/"));
            Assert.AreEqual("/9/1.0/", newSibling.ToString(), );
        }

        [TestMethod]
        public void GetDescendantsAddOne()
        {
            var newSibling = SqlHierarchyId.Parse("/9/").GetDescendant(
                SqlHierarchyId.Parse("/9/1/"),
                SqlHierarchyId.Parse("/9/2/"));
            Assert.AreEqual("/9/1.1/", newSibling.ToString(), );
        }
    }
}

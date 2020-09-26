using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
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
            Assert.AreEqual("/9/2/", newSibling.ToString());
        }

        [TestMethod]
        public void GetDescendantsIncrementFirst()
        {
            var newSibling = SqlHierarchyId.Parse("/9/").GetDescendant(
                SqlHierarchyId.Parse("/9/1.1/"),
                SqlHierarchyId.Parse("/9/2/"));
            Assert.AreEqual("/9/1.2/", newSibling.ToString());
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

        [TestMethod]
        public void GetDescendantsDecrementSecond()
        {
            var newSibling = SqlHierarchyId.Parse("/9/").GetDescendant(
                SqlHierarchyId.Parse("/9/1/"),
                SqlHierarchyId.Parse("/9/1.1/"));
            Assert.AreEqual("/9/1.0/", newSibling.ToString());
        }

        [TestMethod]
        public void GetDescendantsAddOne()
        {
            var newSibling = SqlHierarchyId.Parse("/9/").GetDescendant(
                SqlHierarchyId.Parse("/9/1/"),
                SqlHierarchyId.Parse("/9/2/"));
            Assert.AreEqual("/9/1.1/", newSibling.ToString());
        }

        [TestMethod]
        public void ParseLongNodePositive()
        {
            var expected = "/281479271683151/";
            var result = SqlHierarchyId.Parse(expected);
            Assert.AreEqual(expected, result.ToString());
        }

        [TestMethod]
        public void ParseLongNodeNegative()
        {
            var expected = "/-281479271682120/";
            var result = SqlHierarchyId.Parse(expected);
            Assert.AreEqual(expected, result.ToString());
        }

        [TestMethod]
        public void WriteNull()
        {
            var hnull = SqlHierarchyId.Null;
            AssertEx.ThrowsException(() => hnull.Write(new System.IO.BinaryWriter(new MemoryStream())), typeof(HierarchyIdException), "24002: SqlHierarchyId.Write failed because 'this' was a NULL instance.");
        }

        [TestMethod]
        [WorkItem(45)]
        public void NullToString()
        {
            Assert.AreEqual("NULL", SqlHierarchyId.Null.ToString());
        }

        [TestMethod]
        [WorkItem(45)]
        public void TestToString()
        {
            var h = SqlHierarchyId.Parse("/9/1/");
            Assert.AreEqual("/9/1/", h.ToString());
        }

        [TestMethod]
        [WorkItem(44)]
        public void DefaultIsNull()
        {
            var h = default(SqlHierarchyId);
            Assert.IsTrue(h.IsNull);
        }

        [TestMethod]
        [WorkItem(43)]
        public void ReadResetsNull()
        {
            SqlHierarchyId z = SqlHierarchyId.Null;
            z.Read(new System.IO.BinaryReader(new System.IO.MemoryStream(Convert.FromBase64String("P6T6"))));
            Assert.IsFalse(z.IsNull);  // IsNull property should now be false
        }

        [TestMethod]
        [WorkItem(34)]
        public void ParseVeryLargeHierarchyId()
        {
            string id = "/9138844059576.194933736431247.745612732/136587127227772.29968291099783.2405269301/194815533346310.190518957122630.1754824175/131180557026026.166347272232468.2634227923/112680214461405.155342927909666.4090640326/38488193629220.193847278467647.3890935971/";
            var h = SqlHierarchyId.Parse(id);
            Assert.AreEqual(id, h.ToString());
        }
                [TestMethod]
        public void CreateSiblingOnFirstLevel()
        {
            var a = SqlHierarchyId.Parse("/9/");
            var b = SqlHierarchyId.Parse("/10/");
            Assert.AreEqual("/9.1/", a.GetAncestor(1).GetDescendant(a, b).ToString());
        }
        
        [TestMethod]
        public void CreateSiblingOnFirstLevelWithADistance()
        {
            var a = SqlHierarchyId.Parse("/9/");
            var b = SqlHierarchyId.Parse("/20/");
            Assert.AreEqual("/10/", a.GetAncestor(1).GetDescendant(a, b).ToString());
        }
        
        [TestMethod]
        public void CreateSiblingOnSecondLevel()
        {
            var a = SqlHierarchyId.Parse("/9/1/");
            var b = SqlHierarchyId.Parse("/9/2/");
            Assert.AreEqual("/9/1.1/", a.GetAncestor(1).GetDescendant(a, b).ToString());
        }
        
        [TestMethod]
        public void CreateSiblingOnSecondLevelWithADistance()
        {
            var a = SqlHierarchyId.Parse("/9/1/");
            var b = SqlHierarchyId.Parse("/9/10/");
            Assert.AreEqual("/9/2/", a.GetAncestor(1).GetDescendant(a, b).ToString());
        }
        
        [TestMethod]
        public void CreateSiblingOnDeeperLevel()
        {
            var a = SqlHierarchyId.Parse("/42/1/33.1/1.5/");
            var b = SqlHierarchyId.Parse("/42/1/33.1/1.6/");
            Assert.AreEqual("/42/1/33.1/1.5.1/", a.GetAncestor(1).GetDescendant(a, b).ToString());
        }

        [TestMethod]
        public void CreateSiblingWhenHierarchyIsNegative()
        {
            var a = SqlHierarchyId.Parse("/-1/");
            var b = SqlHierarchyId.Parse("/0/");
            Assert.AreEqual("/-1.1/", a.GetAncestor(1).GetDescendant(a, b).ToString());
        }
        
        [TestMethod]
        public void InsertTopLevelNodeBetweenNodes()
        {
            var id = SqlHierarchyId.GetRoot();
            var idChild = id.GetDescendant(SqlHierarchyId.Parse("/0/"), SqlHierarchyId.Parse("/1/"));
            Assert.AreEqual("/0.1/", idChild.ToString());
        }
    }
}

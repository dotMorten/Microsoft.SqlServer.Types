using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Text;

namespace Microsoft.SqlServer.Types.Tests.Geometry
{
    [TestClass]
    [TestCategory("SqlGeometry")]
    public class SerializeToBinary
    {
        [TestMethod]
        public void TestEmptyPoint()
        {
            var emptyPoint = StreamExtensions.CreateBytes(0, (byte)0x01, (byte)0x04, 0, 0, 1, -1, -1, (byte)0x01);

            var g = SqlGeometry.Parse("POINT EMPTY");

            var serialized = g.Serialize().Value;
            CollectionAssert.AreEqual(emptyPoint, serialized);
        }

        [TestMethod]
        public void TestPoint()
        {
            var point = StreamExtensions.CreateBytes(4326, (byte)0x01, (byte)0x0C, 5d, 10d);
            var g = SqlGeometry.Parse("POINT (5 10)");
            g.STSrid = 4326;

            var serialized = g.Serialize().Value;
            CollectionAssert.AreEqual(point, serialized);
        }

        [TestMethod]
        public void TestLineString()
        {
            var line = StreamExtensions.CreateBytes(4326, (byte)0x01, (byte)0x05,
                3, 0d, 1d, 3d, 2d, 4d, 5d, 1d, 2d, double.NaN, //vertices
                1, (byte)0x01, 0, //figures
                1, -1, 0, (byte)0x02 //shapes
                );
            var g = SqlGeometry.Parse("LINESTRING (0 1 1, 3 2 2, 4 5 NULL)");
            g.STSrid = 4326;

            var serialized = g.Serialize().Value;
            CollectionAssert.AreEqual(line, serialized);
        }

        [TestMethod]
        public void ReadNaNThrows()
        {
            AssertEx.ThrowsException(() => {
                SqlGeometry.Parse("LINESTRING (0 1 1, 3 2 2, 4 5 NaN)");
            }, typeof(FormatException), "24142: Expected \"NULL\" at position 30. The input has \"NaN)\".");
        }

        [TestMethod]
        public void ReadNonNumberThrows()
        {
            AssertEx.ThrowsException(() => {
                SqlGeometry.Parse("LINESTRING (0 1 1, 3 2 2, 4 ABC 9)");
            }, typeof(FormatException), "24141: A number is expected at position 31 of the input. The input has ABC.");
        }

        [TestMethod]
        public void TestEmptyGeometryCollection()
        {
            var coll = StreamExtensions.CreateBytes(4326, (byte)0x01, (byte)0x04,
                0, //vertices
                0, //figures
                1, -1, -1, (byte)0x07 //shapes
            );

            var g = SqlGeometry.Parse("GEOMETRYCOLLECTION EMPTY");
            g.STSrid = 4326;

            var serialized = g.Serialize().Value;
            CollectionAssert.AreEqual(coll, serialized);
        }

        [TestMethod]
        [Ignore]
        [WorkItem(25)]
        public void TestGeometryCollectionWithEmpty()
        {
            var coll = StreamExtensions.CreateBytes(4326, (byte)0x01, (byte)0x04,
                1, 4d, 6d, //vertices
                1, (byte)0x01, 0, //figures
                3, -1, 0, (byte)0x07, 0, 0, (byte)0x00, 0, -1, (byte)0x01 //shapes
            );

            var g = SqlGeometry.Parse("GEOMETRYCOLLECTION(POINT(4 6), POINT EMPTY)");
            g.STSrid = 4326;

            var serialized = g.Serialize().Value;
            CollectionAssert.AreEqual(coll, serialized);
        }
    }
}

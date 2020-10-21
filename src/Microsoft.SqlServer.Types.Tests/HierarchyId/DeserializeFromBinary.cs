using Microsoft.SqlServer.Types;
using Microsoft.SqlServer.Types.SqlHierarchy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Data.SqlClient;
using System.IO;
using System.Text;

namespace Microsoft.SqlServer.Types.Tests.HierarchyId
{
    /// <summary>
    /// Deserialize tests based on examples in the UDT specification
    /// </summary>
    [TestClass]
    [TestCategory("Deserialize")]
    [TestCategory("SqlHierarchyId")]
    public class DeserializeFromBinary
    {

        [TestMethod]
        public void TestSqlHiarchy1()
        {
            // The first child of the root node, with a logical representation of / 1 /, is represented as the following bit sequence:
            // 01011000
            // The first two bits, 01, are the L1 field, meaning that the first node has a label between 0(zero) and 3.The next two bits,
            // 01, are the O1 field and are interpreted as the integer 1.Adding this to the beginning of the range specified by the L1 yields 1.
            // The next bit, with the value 1, is the F1 field, which means that this is a "real" level, with 1 followed by a slash in the logical
            // representation.The final three bits, 000, are the W field, padding the representation to the nearest byte.
            byte[] bytes = { 0x58 }; //01011000
            var hid = new Microsoft.SqlServer.Types.SqlHierarchyId();
            using (var r = new BinaryReader(new MemoryStream(bytes)))
            {
                hid.Read(r);
            }
            Assert.AreEqual("/1/", hid.ToString());
        }

        [TestMethod]
        public void TestSqlHiarchy2()
        {
            // As a more complicated example, the node with logical representation / 1 / -2.18 / (the child with label - 2.18 of the child with label 1 of the root node) is represented as the following sequence of bits(a space has been inserted after every grouping of 8 bits to make the sequence easier to follow):
            // 01011001 11111011 00000101 01000000
            // The first three fields are the same as in the first example.That is, the first two bits(01) are the L1 field, the second two bits(01) are the O1 field, and the fifth bit(1) is the F1 field.This encodes the / 1 / portion of the logical representation.
            // The next 5 bits(00111) are the L2 field, so the next integer is between - 8 and - 1.The following 3 bits(111) are the O2 field, representing the offset 7 from the beginning of this range.Thus, the L2 and O2 fields together encode the integer - 1.The next bit(0) is the F2 field.Because it is 0(zero), this level is fake, and 1 has to be subtracted from the integer yielded by the L2 and O2 fields. Therefore, the L2, O2, and F2 fields together represent -2 in the logical representation of this node.
            // The next 3 bits(110) are the L3 field, so the next integer is between 16 and 79.The subsequent 8 bits(00001010) are the L4 field. Removing the anti - ambiguity bits from there(the third bit(0) and the fifth bit(1)) leaves 000010, which is the binary representation of 2.Thus, the integer encoded by the L3 and O3 fields is 16 + 2, which is 18.The next bit(1) is the F3 field, representing the slash(/) after the 18 in the logical representation.The final 6 bits(000000) are the W field, padding the physical representation to the nearest byte.
            byte[] bytes = { 0x59,0xFB,0x05,0x40 }; //01011001 11111011 00000101 01000000
            var hid = new Microsoft.SqlServer.Types.SqlHierarchyId();
            using (var r = new BinaryReader(new MemoryStream(bytes)))
            {
                hid.Read(r);
            }
            Assert.AreEqual("/1/-2.18/", hid.ToString());
        }

        static bool CompareWithSqlServer = false;

        [DataTestMethod]
        [DataRow("/-4294971464/")]
        [DataRow("/4294972495/")]
        [DataRow("/3.2725686107/")]
        [DataRow("/0/")]
        [DataRow("/1/")]
        [DataRow("/1.0.2/")]
        [DataRow("/1.1.2/")]
        [DataRow("/1.2.2/")]
        [DataRow("/1.3.2/")]
        [DataRow("/3.0/")]
        public void SerializeDeserialize(string route)
        {
            var parsed = SqlHierarchyId.Parse(route);
            var ms = new MemoryStream();
            parsed.Write(new BinaryWriter(ms));
            ms.Position = 0;
            var dumMem = Dump(ms);
            ms.Position = 0;
            var roundTrip = new Microsoft.SqlServer.Types.SqlHierarchyId();
            roundTrip.Read(new BinaryReader(ms));
            if (parsed != roundTrip)
                Assert.AreEqual(parsed, roundTrip);

            if (CompareWithSqlServer)
            {
                /*
                 CREATE TABLE [dbo].[TreeNode](
	                [Id] [int] IDENTITY(1,1) NOT NULL,
	                [Route] [hierarchyid] NOT NULL
                 )
                */
                using (SqlConnection con = new SqlConnection(@"Data Source=.\SQLEXPRESS;Initial Catalog=HierarchyTest;Integrated Security=true"))
                {
                    con.Open();
                    var id = new SqlCommand($"INSERT INTO [dbo].[TreeNode] (Route) output INSERTED.ID VALUES ('{route}') ", con).ExecuteScalar();

                    using (var reader = new SqlCommand($"SELECT Route FROM [dbo].[TreeNode] WHERE ID = " + id, con).ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var sqlRoundTrip = new Microsoft.SqlServer.Types.SqlHierarchyId();
                            var dumSql = Dump(reader.GetStream(0));
                            Assert.AreEqual(dumMem, dumSql);
                            sqlRoundTrip.Read(new BinaryReader(reader.GetStream(0)));
                            if (parsed != sqlRoundTrip)
                                Assert.AreEqual(parsed, sqlRoundTrip);
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void DeserializeRandom()
        {
            Random r = new Random();
            for (int i = 0; i < 10000; i++)
            {
                SerializeDeserialize(RandomHierarhyId(r));
            }
        }

        public static string RandomHierarhyId(Random random)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("/");
            var levels = random.Next(4);
            for (int i = 0; i < levels; i++)
            {
                var subLevels = random.Next(1, 4);
                for (int j = 0; j < subLevels; j++)
                {
                    var pattern = KnownPatterns.RandomPattern(random);
                    sb.Append(random.NextLong(pattern.MinValue, pattern.MaxValue + 1).ToString());
                    if (j < subLevels - 1)
                        sb.Append(".");
                }
                sb.Append("/");
            }

            return sb.ToString();
        }


        static string Dump(Stream ms)
        {
            return new BitReader(new BinaryReader(ms)).ToString();
        }
    }

    public static class RandomExtensionMethods
    {
        /// <summary>
        /// Returns a random long from min (inclusive) to max (exclusive)
        /// </summary>
        /// <param name="random">The given random instance</param>
        /// <param name="min">The inclusive minimum bound</param>
        /// <param name="max">The exclusive maximum bound.  Must be greater than min</param>
        public static long NextLong(this Random random, long min, long max)
        {
            if (max <= min)
                throw new ArgumentOutOfRangeException("max", "max must be > min!");

            //Working with ulong so that modulo works correctly with values > long.MaxValue
            ulong uRange = (ulong)(max - min);

            //Prevent a modolo bias; see https://stackoverflow.com/a/10984975/238419
            //for more information.
            //In the worst case, the expected number of calls is 2 (though usually it's
            //much closer to 1) so this loop doesn't really hurt performance at all.
            ulong ulongRand;
            do
            {
                byte[] buf = new byte[8];
                random.NextBytes(buf);
                ulongRand = (ulong)BitConverter.ToInt64(buf, 0);
            } while (ulongRand > ulong.MaxValue - ((ulong.MaxValue % uRange) + 1) % uRange);

            return (long)(ulongRand % uRange) + min;
        }
    }
    }

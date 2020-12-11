using Microsoft.SqlServer.Types.SqlHierarchy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Text;

namespace Microsoft.SqlServer.Types.Tests.HierarchyId
{
    [TestCategory("Database")]
    [TestCategory("SqlHierarchyId")]
    [TestClass]
    public class HierarchyDbTests
    {
        const string DataSource = @"Data Source=(localdb)\mssqllocaldb;Integrated Security=True;AttachDbFileName=";

        private static SqlConnection _connection;
        private static string _path = null!;

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            if (_path == null)
            {
                _path = Path.Combine(new FileInfo(typeof(HierarchyDbTests).Assembly.Location).Directory.FullName, "HierarchyUnitTestData.mdf");
                if (File.Exists(_path))
                    File.Delete(_path);
                DatabaseUtil.CreateSqlDatabase(_path);
                using (var conn = new SqlConnection(DataSource + _path))
                {
                    conn.Open();
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "CREATE TABLE [dbo].[TreeNode]([Id] [int] IDENTITY(1,1) NOT NULL, [Route] [hierarchyid] NOT NULL);";
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }

                _connection = new SqlConnection(ConnectionString);
                _connection.Open();
            }
        }

        private static string ConnectionString => DataSource + _path;

        [ClassCleanup]
        public static void ClassCleanup()
        {
            _connection.Close();
            _connection.Dispose();
        }

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
                Assert.AreEqual(parsed, roundTrip); //breakpoint here

            var id = new SqlCommand($"INSERT INTO [dbo].[TreeNode] (Route) output INSERTED.ID VALUES ('{route}') ", _connection).ExecuteScalar();

            using (var reader = new SqlCommand($"SELECT Route FROM [dbo].[TreeNode] WHERE ID = " + id, _connection).ExecuteReader())
            {
                while (reader.Read())
                {
                    var sqlRoundTrip = new Microsoft.SqlServer.Types.SqlHierarchyId();
                    var dumSql = Dump(reader.GetStream(0));
                    Assert.AreEqual(dumMem, dumSql);
                    sqlRoundTrip.Read(new BinaryReader(reader.GetStream(0)));
                    if (parsed != sqlRoundTrip)
                        Assert.AreEqual(parsed, sqlRoundTrip); //breakpoint here
                }
            }
        }

        [DataTestMethod]
        [DynamicData(nameof(GetData), DynamicDataSourceType.Method)]
        public void SerializeDeserializeRandom(string route)
        {
            SerializeDeserialize(route);
        }

        private const int CountOfGeneratedCases = 1000;
        public static IEnumerable<object[]> GetData()
        {
            Random r = new Random();
            for (var i = 0; i < CountOfGeneratedCases; i++)
            {
                yield return new object[] { RandomHierarhyId(r)};
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

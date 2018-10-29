using Microsoft.SqlServer.Server;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Microsoft.SqlServer.Types.Tests.Geometry
{
    [TestClass]
    [TestCategory("Database")]
    [TestCategory("SqlGeometry")]
    public class DBTests : IDisposable
    {
        const string connstr = @"Data Source=(localdb)\mssqllocaldb;Integrated Security=True;AttachDbFileName=";

        private System.Data.SqlClient.SqlConnection conn;
        private static string path;
        static DBTests()
        {
            path = Path.Combine(new FileInfo(typeof(DBTests).Assembly.Location).Directory.FullName, "UnitTestData.mdf");
            CreateSqlDatabase(path);
            using (var conn = new System.Data.SqlClient.SqlConnection(connstr + path))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = OgcConformanceMap.DropTables;
                cmd.ExecuteNonQuery();
                cmd.CommandText = OgcConformanceMap.CreateTables;
                cmd.ExecuteNonQuery();
                cmd.CommandText = OgcConformanceMap.CreateRows;
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }

        public static string ConnectionString => connstr + path;


        public DBTests()
        {
            conn = new System.Data.SqlClient.SqlConnection(ConnectionString);
            conn.Open();
        }

        private static void CreateSqlDatabase(string filename)
        {
            string databaseName = System.IO.Path.GetFileNameWithoutExtension(filename);
            if (File.Exists(filename))
                File.Delete(filename);
            if (File.Exists(filename.Replace(".mdf","_log.ldf")))
                File.Delete(filename.Replace(".mdf", "_log.ldf"));
            using (var connection = new System.Data.SqlClient.SqlConnection(
                @"Data Source=(localdb)\mssqllocaldb;Initial Catalog=master; Integrated Security=true;"))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        String.Format("CREATE DATABASE {0} ON PRIMARY (NAME={0}, FILENAME='{1}')", databaseName, filename);
                    command.ExecuteNonQuery();

                    command.CommandText =
                        String.Format("EXEC sp_detach_db '{0}', 'true'", databaseName);
                    command.ExecuteNonQuery();
                }
            }
        }
        public void Dispose()
        {
            conn.Close();
            conn.Dispose();
        }

        [TestMethod]
        public void QueryPoints()
        {
            string[] lineTables = new[] { "bridges", "buildings" };
            foreach (var table in lineTables)
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $"SELECT * FROM {table}";
                    using (var reader = cmd.ExecuteReader())
                    {
                        int geomColumn = 0;
                        while (!reader.GetDataTypeName(geomColumn).EndsWith(".geometry"))
                            geomColumn++;
                        while (reader.Read())
                        {
                            var geomValue = SqlGeometry.Deserialize(reader.GetSqlBytes(geomColumn));
                            //Assert.IsInstanceOfType(<SqlGeometry>(geomValue);
                            var g = geomValue as SqlGeometry;
                            Assert.IsFalse(g.IsNull);
                            Assert.AreEqual("POINT", g.STGeometryType().Value, true);
                            Assert.AreEqual(1, g.STNumGeometries().Value);
                            Assert.AreEqual(101, g.STSrid);
                            Assert.IsFalse(g.STX.IsNull);
                            Assert.IsFalse(g.STY.IsNull);
                            Assert.IsTrue(g.Z.IsNull);
                            Assert.IsTrue(g.M.IsNull);
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void QueryLineStrings()
        {
            string[] lineTables = new[] { "road_segments", "streams" };
            foreach (var table in lineTables)
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $"SELECT * FROM {table}";
                    using (var reader = cmd.ExecuteReader())
                    {
                        int geomColumn = 0;
                        while (!reader.GetDataTypeName(geomColumn).EndsWith(".geometry")) 
                            geomColumn++;
                        while (reader.Read())
                        {
                            var geomValue = SqlGeometry.Deserialize(reader.GetSqlBytes(geomColumn));
                            Assert.IsInstanceOfType(geomValue, typeof(SqlGeometry));
                            var g = geomValue as SqlGeometry;
                            Assert.IsFalse(g.IsNull);
                            Assert.AreEqual(101, g.STSrid);
                            Assert.AreEqual(1, g.STNumGeometries().Value);
                            Assert.AreEqual("LINESTRING", g.STGeometryType().Value, true);
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void QueryMultiLineStrings()
        {
            string[] lineTables = new[] { "divided_routes" };
            foreach (var table in lineTables)
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $"SELECT * FROM {table}";
                    using (var reader = cmd.ExecuteReader())
                    {
                        int geomColumn = 0;
                        while (!reader.GetDataTypeName(geomColumn).EndsWith(".geometry"))
                            geomColumn++;
                        while (reader.Read())
                        {
                            var geomValue = SqlGeometry.Deserialize(reader.GetSqlBytes(geomColumn));
                            Assert.IsInstanceOfType(geomValue, typeof(SqlGeometry));
                            var g = geomValue as SqlGeometry;
                            Assert.IsFalse(g.IsNull);
                            Assert.AreEqual(101, g.STSrid);
                            Assert.AreEqual(2, g.STNumGeometries().Value);
                            Assert.AreEqual("MULTILINESTRING", g.STGeometryType().Value, true);
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void QueryPolygons()
        {
            string[] lineTables = new[] { "lakes", "buildings", "named_places" };
            foreach (var table in lineTables)
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $"SELECT * FROM {table}";
                    using (var reader = cmd.ExecuteReader())
                    {
                        int geomColumn = 0;
                        while (!reader.GetDataTypeName(geomColumn).EndsWith(".geometry"))
                            geomColumn++;
                        if (table == "buildings") geomColumn++; //this table has two columns. Second is polygons
                        while (reader.Read())
                        {
                            var geomValue = reader.GetValue(geomColumn) as SqlGeometry;
                            //var geomValue = SqlGeometry.Deserialize(reader.GetSqlBytes(geomColumn));
                            Assert.IsInstanceOfType(geomValue, typeof(SqlGeometry));
                            var g = geomValue as SqlGeometry;
                            Assert.IsFalse(g.IsNull);
                            Assert.AreEqual(101, g.STSrid);
                            Assert.AreEqual(1, g.STNumGeometries().Value);
                            Assert.AreEqual("POLYGON", g.STGeometryType().Value.ToUpper());
                        }
                    }
                }
            }
        }
        [TestMethod]
       public void QueryMultiPolygons()
        {
            string[] lineTables = new[] {"forests", "ponds" };
            foreach (var table in lineTables)
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $"SELECT * FROM {table}";
                    using (var reader = cmd.ExecuteReader())
                    {
                        int geomColumn = 0;
                        while (!reader.GetDataTypeName(geomColumn).EndsWith(".geometry"))
                            geomColumn++;
                        while (reader.Read())
                        {
                            var geomValue = SqlGeometry.Deserialize(reader.GetSqlBytes(geomColumn));
                            Assert.IsInstanceOfType(geomValue, typeof(SqlGeometry));
                            var g = geomValue as SqlGeometry;
                            Assert.IsFalse(g.IsNull);
                            Assert.AreEqual(101, g.STSrid);
                            Assert.AreEqual(2, g.STNumGeometries().Value);
                            Assert.AreEqual("MULTIPOLYGON", g.STGeometryType().Value.ToUpper());
                        }
                    }
                }
            }
        }

        [TestMethod]
        [TestCategory("SqlHierarchyId")]
        public void QuerySqlHierarchyId()
        {
            List<SqlHierarchyId> hierarchyIds = new List<SqlHierarchyId>();
            StringBuilder ssb = new StringBuilder();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = $"SELECT OrgNode.ToString(), OrgNode FROM employees";
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var str = reader.IsDBNull(0) ? null : reader.GetString(0);
                        var value = reader.GetValue(1);
                        if(!reader.IsDBNull(0))
                            Assert.IsInstanceOfType(value, typeof(SqlHierarchyId));
                        var sqlHierId = reader.IsDBNull(1) ? (SqlHierarchyId?)null : reader.GetFieldValue<SqlHierarchyId>(1);

                        Assert.AreEqual(str, sqlHierId?.ToString());

                        if (sqlHierId.HasValue)
                        {
                            var should = reader.GetSqlBytes(1).Value;
                            SqlBytes current;
                            using (var ms = new MemoryStream())
                            {
                                sqlHierId.Value.Write(new BinaryWriter(ms));
                                current = new SqlBytes(ms.ToArray());
                            }
                            Assert.AreEqual(should.Length, current.Length);
                            for (int i = 0; i < should.Length; i++)
                            {
                                Assert.AreEqual(should[i], current[i]);
                            }

                            hierarchyIds.Add(sqlHierId.Value);
                        }
                    }
                }
            }

            foreach (var shi in hierarchyIds)
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $"SELECT Count(*) FROM employees WHERE OrgNode = @p";
                    var p = cmd.CreateParameter();
                    p.SqlDbType = SqlDbType.Udt;
                    p.UdtTypeName = "HierarchyId";
                    p.ParameterName = "@p";
                    p.Value = shi;
                    cmd.Parameters.Add(p);

                    Assert.AreEqual(1, cmd.ExecuteScalar());
                }
            }
        }
    }
}

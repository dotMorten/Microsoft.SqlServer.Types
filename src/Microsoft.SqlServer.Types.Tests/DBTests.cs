using Microsoft.SqlServer.Server;
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
using Xunit;

namespace Microsoft.SqlServer.Types.Tests
{
    public class DBTests : IDisposable
    {
        const string connstr = @"Data Source=(localdb)\mssqllocaldb;Integrated Security=True;AttachDbFileName=";

        private System.Data.SqlClient.SqlConnection conn;
        private static string path;
        static DBTests()
        {
            path = Path.Combine(new FileInfo(typeof(DBTests).Assembly.Location).Directory.FullName, "UnitTestData.mdf");
            CreateSqlDatabase(path);
            var conn = new System.Data.SqlClient.SqlConnection(connstr + path);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = OgcConformanceMap.DropTables;
            cmd.ExecuteNonQuery();
            cmd.CommandText = OgcConformanceMap.CreateTables;
            cmd.ExecuteNonQuery();
            cmd.CommandText = OgcConformanceMap.CreateRows;
            cmd.ExecuteNonQuery();
        }
        
        public DBTests()
        {
            conn = new System.Data.SqlClient.SqlConnection(connstr + path);
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

        [Fact]
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
                            var geom = reader.GetSqlBytes(geomColumn);
                            var g = SqlGeometry.Deserialize(geom);
                            Assert.False(g.IsNull);
                            Assert.Equal("POINT", g.STGeometryType().Value, true);
                            Assert.Equal(1, g.STNumGeometries().Value);
                            Assert.Equal(101, g.STSrid);
                            Assert.False(g.STX.IsNull);
                            Assert.False(g.STY.IsNull);
                            Assert.True(g.Z.IsNull);
                            Assert.True(g.M.IsNull);
                        }
                    }
                }
            }
        }

        [Fact]
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
                            var geom = reader.GetSqlBytes(geomColumn);
                            var g = SqlGeometry.Deserialize(geom);
                            Assert.False(g.IsNull);
                            Assert.Equal(101, g.STSrid);
                            Assert.Equal(1, g.STNumGeometries().Value);
                            Assert.Equal("LINESTRING", g.STGeometryType().Value, true);
                        }
                    }
                }
            }
        }

        [Fact]
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
                            var geom = reader.GetSqlBytes(geomColumn);
                            var g = SqlGeometry.Deserialize(geom);
                            Assert.False(g.IsNull);
                            Assert.Equal(101, g.STSrid);
                            Assert.Equal(2, g.STNumGeometries().Value);
                            Assert.Equal("MULTILINESTRING", g.STGeometryType().Value, true);
                        }
                    }
                }
            }
        }

        [Fact]
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
                            var geom = reader.GetSqlBytes(geomColumn);
                            var g = SqlGeometry.Deserialize(geom);
                            Assert.False(g.IsNull);
                            Assert.Equal(101, g.STSrid);
                            Assert.Equal(1, g.STNumGeometries().Value);
                            Assert.Equal("POLYGON", g.STGeometryType().Value.ToUpper());
                        }
                    }
                }
            }
        }
        [Fact]
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
                            var geom = reader.GetSqlBytes(geomColumn);
                            var g = SqlGeometry.Deserialize(geom);
                            Assert.False(g.IsNull);
                            Assert.Equal(101, g.STSrid);
                            Assert.Equal(2, g.STNumGeometries().Value);
                            Assert.Equal("MULTIPOLYGON", g.STGeometryType().Value.ToUpper());
                        }
                    }
                }
            }
        }

        [Fact]
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
                        var sqlHierId = reader.IsDBNull(1) ? (SqlHierarchyId?)null : reader.GetFieldValue<SqlHierarchyId>(1);

                        Assert.Equal(str, sqlHierId?.ToString());

                        if (sqlHierId.HasValue)
                        {
                            var should = reader.GetSqlBytes(1).Value;
                            SqlBytes current;
                            using (var ms = new MemoryStream())
                            {
                                sqlHierId.Value.Write(new BinaryWriter(ms));
                                current = new SqlBytes(ms.ToArray());
                            }
                            Assert.Equal(should.Length, current.Length);
                            for (int i = 0; i < should.Length; i++)
                            {
                                Assert.Equal(should[i], current[i]);
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

                    Assert.Equal(1, cmd.ExecuteScalar());
                }
            }
        }
    }
    internal static class StreamExtensions
    {
        public static string ToBinaryString(this byte[] bytes)
        {
            var result = string.Join(" ", bytes.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')));
            return result;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
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
    }
}

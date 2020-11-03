using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Microsoft.SqlServer.Types.Tests
{
    static class DatabaseUtil
    {
        internal static void CreateSqlDatabase(string filename)
        {
            string databaseName = System.IO.Path.GetFileNameWithoutExtension(filename);
            if (File.Exists(filename))
                File.Delete(filename);
            if (File.Exists(filename.Replace(".mdf", "_log.ldf")))
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
    }
}

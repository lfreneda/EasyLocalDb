using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlLocalDb;
using System.IO;

namespace EasyLocalDb
{
    public class LocalDb
    {
        protected readonly string DbName;
        protected readonly string InstanceName;

        private readonly string _attachDbFileName;
        private readonly string _tempBasePath;

        public LocalDb(string tempBasePath = @"C:\temp\", string instanceName = "v11.0")
        {
            InstanceName = instanceName;
            _tempBasePath = tempBasePath;

            ISqlLocalDbApi localDb = new SqlLocalDbApiWrapper();

            if (!localDb.IsLocalDBInstalled())
            {
                throw new SqlLocalDbException("LocalDb is not installed.");
            }

            CreateDirectoriesIfNotExists();

            DbName = RandomName();
            _attachDbFileName = Path.Combine(_tempBasePath, DbName) + ".mdf";
        }

        private void CreateDirectoriesIfNotExists()
        {
            if (!Directory.Exists(_tempBasePath))
            {
                Directory.CreateDirectory(_tempBasePath);
            }
        }

        private string RandomName()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 8);
        }

        protected string GetConnectionString()
        {
            return
                string.Format(
                    @"Data Source=(LocalDb)\{2};AttachDBFileName={1};Initial Catalog={0};Integrated Security=True;",
                    DbName, _attachDbFileName, InstanceName);
        }

        private string GetMasterConnectionString()
        {
            var connectionString =
                string.Format(@"Data Source=(LocalDb)\" + InstanceName +
                              ";Initial Catalog=master;Integrated Security=True");
            return connectionString;
        }

        public void CreateDatabase()
        {
            using (var connection = new SqlConnection(GetMasterConnectionString()))
            {
                connection.Open();

                try
                {
                    var createCommand = string.Format("CREATE DATABASE [{0}] ON (NAME = N'{0}', FILENAME = '{1}')",
                        DbName, _attachDbFileName);
                    using (var command = new SqlCommand(createCommand, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        private void DetachDatabase(string databaseName)
        {
            using (var connection = new SqlConnection(GetMasterConnectionString()))
            {
                connection.Open();

                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = string.Format("exec sp_detach_db '{0}'", databaseName);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public IEnumerable<string> GetDatabasesOnLocalDb()
        {
            var results = new List<string>();

            using (var connection = new SqlConnection(GetMasterConnectionString()))
            {
                connection.Open();

                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText =
                        "SELECT name FROM master..sysdatabases where name not in ('master', 'model', 'msdb', 'tempdb')";

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            results.Add(reader.GetString(0));
                        }
                    }
                }
            }

            return results;
        }

        protected void CleanUp()
        {
            foreach (var databasesName in GetDatabasesOnLocalDb())
            {
                try
                {
                    DetachDatabase(databasesName);
                }
                catch (Exception)
                {
                }
            }

            var files = Directory.GetFiles(_tempBasePath);
            foreach (var file in files)
            {
                try
                {
                    File.Delete(file);
                }
                catch
                {
                }
            }
        }
    }
}
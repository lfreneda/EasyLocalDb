using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlLocalDb;
using System.IO;

namespace EasyLocalDb
{
    public class LocalDb : IDisposable
    {
        private readonly string _attachDbFileName;
        private readonly string _dbName;
        private readonly string _instanceName;
        private readonly string _tempBasePath;
        private bool _disposabled;

        public LocalDb(string tempBasePath = @"C:\temp\", string instanceName = "v11.0")
        {
            _instanceName = instanceName;
            _tempBasePath = tempBasePath;

            ISqlLocalDbApi localDb = new SqlLocalDbApiWrapper();

            if (!localDb.IsLocalDBInstalled())
            {
                throw new SqlLocalDbException("LocalDb is not installed.");
            }

            CreateDirectoriesIfNotExists();

            _dbName = RandomName();
            _attachDbFileName = Path.Combine(_tempBasePath, _dbName) + ".mdf";
        }

        public void Dispose()
        {
            if (!_disposabled) CleanUp();
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
                    _dbName, _attachDbFileName, _instanceName);
        }

        private string GetMasterConnectionString()
        {
            var connectionString =
                string.Format(@"Data Source=(LocalDb)\" + _instanceName +
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
                        _dbName, _attachDbFileName);
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

        public void CleanUp()
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

            _disposabled = true;
        }
    }
}
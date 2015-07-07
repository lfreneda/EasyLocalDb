using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FluentMigrator;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Initialization;

namespace EasyLocalDb.Sample.Migrations.Lib
{
    public class Migrator
    {
        readonly string _connectionString;
        readonly string _dbType;
        readonly Assembly _migrationAssembly;

        public Migrator(string connectionString, string dbType, Assembly migrationAssembly)
        {
            _connectionString = connectionString;
            _dbType = dbType;
            _migrationAssembly = migrationAssembly;
        }

        private class MigrationOptions : IMigrationProcessorOptions
        {
            public bool PreviewOnly { get; set; }
            public int Timeout { get; set; }
            public string ProviderSwitches { get; set; }
        }

        public void Migrate(Action<IMigrationRunner> runnerAction)
        {
            var options = new MigrationOptions { PreviewOnly = false, Timeout = 0 };
            var factory = new FluentMigrator.Runner.Processors.MigrationProcessorFactoryProvider().GetFactory(_dbType);
            var announcer = new TextWriterAnnouncer(s => System.Diagnostics.Debug.WriteLine(s));
            var migrationContext = new RunnerContext(announcer);
            var processor = factory.Create(_connectionString, announcer, options);
            var runner = new MigrationRunner(_migrationAssembly, migrationContext, processor);
            runnerAction(runner);
        }
    }
}

using NUnit.Framework;
using EasyLocalDb.Sample.Migrations;
using EasyLocalDb.Sample.Migrations.Lib;

namespace EasyLocalDb.Sample
{
    [TestFixture]
    public class MigrationTests : LocalDb
    {
        [SetUp]
        public void SetUp()
        {
            CreateDatabase();
        }

        [Test]
        public void MigrateUp()
        {
            var migrator = new Migrator(GetConnectionString(), "sqlserver", typeof(CreateUsersTableMigration).Assembly);
            migrator.Migrate(runner => runner.MigrateUp());
        }

        [TearDown]
        public void TearDown()
        {
            CleanUp();
        }
    }
}

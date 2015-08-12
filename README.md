# EasyLocalDb
A higher-level interface for LocalDb

### What is it for?
You can create a local temp database to run your integration tests, for example, if you're working with FluentMigrator:

```csharp
    [TestFixture]
    public class MigrationTests : LocalDb /* <----- you should inherits from LocalDb */
    {
        [SetUp]
        public void SetUp()
        {
            CreateDatabase(); /* <----- create a local database */
        }

        [Test]
        public void MigrateUp()
        {
            var connectionString = GetConnectionString(); /* <----- grab local database connection string */
            var migrator = new Migrator(connectionString, "sqlserver", typeof(CreateUsersTableMigration).Assembly);
            migrator.Migrate(runner => runner.MigrateUp());
        }

        [TearDown]
        public void TearDown()
        {
            CleanUp(); /* <----- drop and delete local database */
        }
    }
```

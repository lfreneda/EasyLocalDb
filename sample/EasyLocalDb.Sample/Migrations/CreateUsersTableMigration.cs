using FluentMigrator;

namespace EasyLocalDb.Sample.Migrations
{
    [Migration(1)]
    public class CreateUsersTableMigration : Migration
    {
        public override void Up()
        {
            Create.Table("Users")
                .WithColumn("UserId").AsInt32().PrimaryKey().Identity()
                .WithColumn("Name").AsString()
                .WithColumn("Email").AsAnsiString(128).NotNullable();

            Create.Index().OnTable("Users").OnColumn("Email").Unique();
        }

        public override void Down()
        {
            Delete.Table("Users");
        }
    }
}

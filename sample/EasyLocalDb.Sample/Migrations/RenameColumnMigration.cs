using FluentMigrator;

namespace EasyLocalDb.Sample.Migrations
{
    [Migration(2)]
    public class RenameColumnMigration : Migration
    {
        public override void Up()
        {
            Rename.Column("Email").OnTable("Users").To("EmailAddress");
        }

        public override void Down()
        {
            Rename.Column("EmailAddress").OnTable("Users").To("Email");
        }
    }
}
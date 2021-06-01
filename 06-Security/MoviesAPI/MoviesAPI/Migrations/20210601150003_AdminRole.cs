using Microsoft.EntityFrameworkCore.Migrations;

namespace MoviesAPI.Migrations
{
    public partial class AdminRole : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"INSERT INTO AspNetRoles (Id, [name], [NormalizedName])
                                    VALUES ('3ca9b615-1925-4a48-b116-cba2105b96b8', 'Admin', 'Admin')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DELETE AspNetRoles WHERE Id = '3ca9b615-1925-4a48-b116-cba2105b96b8'");
        }
    }
}

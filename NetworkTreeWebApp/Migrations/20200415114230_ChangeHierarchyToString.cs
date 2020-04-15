using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.SqlServer.Types;

namespace NetworkTreeWebApp.Migrations
{
    public partial class ChangeHierarchyToString : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Level",
                table: "AccountHierarchy");

            migrationBuilder.AddColumn<string>(
                name: "LevelPath",
                table: "AccountHierarchy",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AccountHierarchy",
                keyColumn: "Id",
                keyValue: 1L,
                column: "LevelPath",
                value: "/");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LevelPath",
                table: "AccountHierarchy");

            migrationBuilder.AddColumn<SqlHierarchyId>(
                name: "Level",
                table: "AccountHierarchy",
                type: "hierarchyid",
                nullable: false,
                defaultValue: Microsoft.SqlServer.Types.SqlHierarchyId.Parse(new System.Data.SqlTypes.SqlString("/")));
        }
    }
}

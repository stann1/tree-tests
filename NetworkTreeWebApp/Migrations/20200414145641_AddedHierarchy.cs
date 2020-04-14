using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.SqlServer.Types;

namespace NetworkTreeWebApp.Migrations
{
    public partial class AddedHierarchy : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccountHierarchy",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(unicode: false, maxLength: 50, nullable: false),
                    PlacementPreference = table.Column<int>(nullable: false),
                    Leg = table.Column<int>(nullable: false),
                    ParentId = table.Column<long>(nullable: true),
                    UplinkId = table.Column<long>(nullable: true),
                    Level = table.Column<SqlHierarchyId>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountHierarchy", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountHierarchy_Self_ParentId",
                        column: x => x.ParentId,
                        principalTable: "AccountHierarchy",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccountHierarchy_Self_UplinkId",
                        column: x => x.UplinkId,
                        principalTable: "AccountHierarchy",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "Account",
                keyColumn: "Id",
                keyValue: 2L,
                column: "PlacementPreference",
                value: 2);

            migrationBuilder.UpdateData(
                table: "Account",
                keyColumn: "Id",
                keyValue: 5L,
                column: "PlacementPreference",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Account",
                keyColumn: "Id",
                keyValue: 7L,
                column: "PlacementPreference",
                value: 2);

            migrationBuilder.UpdateData(
                table: "Account",
                keyColumn: "Id",
                keyValue: 11L,
                column: "PlacementPreference",
                value: 3);

            migrationBuilder.UpdateData(
                table: "Account",
                keyColumn: "Id",
                keyValue: 12L,
                column: "PlacementPreference",
                value: 1);

            migrationBuilder.CreateIndex(
                name: "IX_AccountHierarchy_ParentId",
                table: "AccountHierarchy",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountHierarchy_UplinkId",
                table: "AccountHierarchy",
                column: "UplinkId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountHierarchy");

            migrationBuilder.UpdateData(
                table: "Account",
                keyColumn: "Id",
                keyValue: 2L,
                column: "PlacementPreference",
                value: 3);

            migrationBuilder.UpdateData(
                table: "Account",
                keyColumn: "Id",
                keyValue: 5L,
                column: "PlacementPreference",
                value: 3);

            migrationBuilder.UpdateData(
                table: "Account",
                keyColumn: "Id",
                keyValue: 7L,
                column: "PlacementPreference",
                value: 3);

            migrationBuilder.UpdateData(
                table: "Account",
                keyColumn: "Id",
                keyValue: 11L,
                column: "PlacementPreference",
                value: 2);

            migrationBuilder.UpdateData(
                table: "Account",
                keyColumn: "Id",
                keyValue: 12L,
                column: "PlacementPreference",
                value: 2);
        }
    }
}

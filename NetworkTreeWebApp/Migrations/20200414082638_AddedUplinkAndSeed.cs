using Microsoft.EntityFrameworkCore.Migrations;

namespace NetworkTreeWebApp.Migrations
{
    public partial class AddedUplinkAndSeed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "UplinkId",
                table: "Account",
                nullable: true);

            migrationBuilder.InsertData(
                table: "Account",
                columns: new[] { "Id", "Leg", "Name", "ParentId", "PlacementPreference", "UplinkId" },
                values: new object[] { 1L, 0, "A", null, 3, null });

            migrationBuilder.InsertData(
                table: "Account",
                columns: new[] { "Id", "Leg", "Name", "ParentId", "PlacementPreference", "UplinkId" },
                values: new object[] { 2L, 0, "B", 1L, 3, 1L });

            migrationBuilder.InsertData(
                table: "Account",
                columns: new[] { "Id", "Leg", "Name", "ParentId", "PlacementPreference", "UplinkId" },
                values: new object[] { 3L, 0, "C", 1L, 3, 1L });

            migrationBuilder.InsertData(
                table: "Account",
                columns: new[] { "Id", "Leg", "Name", "ParentId", "PlacementPreference", "UplinkId" },
                values: new object[,]
                {
                    { 4L, 0, "D", 2L, 3, 1L },
                    { 7L, 0, "F", 2L, 3, 1L },
                    { 5L, 0, "H", 3L, 3, 1L },
                    { 6L, 0, "K", 3L, 3, 1L }
                });

            migrationBuilder.InsertData(
                table: "Account",
                columns: new[] { "Id", "Leg", "Name", "ParentId", "PlacementPreference", "UplinkId" },
                values: new object[,]
                {
                    { 8L, 0, "G", 4L, 3, 1L },
                    { 9L, 0, "V", 4L, 3, 1L },
                    { 11L, 0, "Q", 7L, 2, 2L },
                    { 10L, 0, "L", 6L, 3, 1L }
                });

            migrationBuilder.InsertData(
                table: "Account",
                columns: new[] { "Id", "Leg", "Name", "ParentId", "PlacementPreference", "UplinkId" },
                values: new object[] { 12L, 0, "X", 11L, 2, 2L });

            migrationBuilder.InsertData(
                table: "Account",
                columns: new[] { "Id", "Leg", "Name", "ParentId", "PlacementPreference", "UplinkId" },
                values: new object[] { 13L, 0, "Y", 11L, 2, 2L });

            migrationBuilder.CreateIndex(
                name: "IX_Account_UplinkId",
                table: "Account",
                column: "UplinkId");

            migrationBuilder.AddForeignKey(
                name: "FK_Account_Self_UplinkId",
                table: "Account",
                column: "UplinkId",
                principalTable: "Account",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Account_Self_UplinkId",
                table: "Account");

            migrationBuilder.DropIndex(
                name: "IX_Account_UplinkId",
                table: "Account");

            migrationBuilder.DeleteData(
                table: "Account",
                keyColumn: "Id",
                keyValue: 5L);

            migrationBuilder.DeleteData(
                table: "Account",
                keyColumn: "Id",
                keyValue: 8L);

            migrationBuilder.DeleteData(
                table: "Account",
                keyColumn: "Id",
                keyValue: 9L);

            migrationBuilder.DeleteData(
                table: "Account",
                keyColumn: "Id",
                keyValue: 10L);

            migrationBuilder.DeleteData(
                table: "Account",
                keyColumn: "Id",
                keyValue: 12L);

            migrationBuilder.DeleteData(
                table: "Account",
                keyColumn: "Id",
                keyValue: 13L);

            migrationBuilder.DeleteData(
                table: "Account",
                keyColumn: "Id",
                keyValue: 4L);

            migrationBuilder.DeleteData(
                table: "Account",
                keyColumn: "Id",
                keyValue: 6L);

            migrationBuilder.DeleteData(
                table: "Account",
                keyColumn: "Id",
                keyValue: 11L);

            migrationBuilder.DeleteData(
                table: "Account",
                keyColumn: "Id",
                keyValue: 3L);

            migrationBuilder.DeleteData(
                table: "Account",
                keyColumn: "Id",
                keyValue: 7L);

            migrationBuilder.DeleteData(
                table: "Account",
                keyColumn: "Id",
                keyValue: 2L);

            migrationBuilder.DeleteData(
                table: "Account",
                keyColumn: "Id",
                keyValue: 1L);

            migrationBuilder.DropColumn(
                name: "UplinkId",
                table: "Account");
        }
    }
}

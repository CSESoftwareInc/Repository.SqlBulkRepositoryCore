using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CSESoftware.Repository.SqlBulkRepositoryCore.TestDatabase.Migrations
{
    public partial class ColumnNameMismatch : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "HomeId",
                table: "FamilyTrees",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Home_Alpha",
                columns: table => new
                {
                    HomeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Home_Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Home_Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Home_Alpha", x => x.HomeId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FamilyTrees_HomeId",
                table: "FamilyTrees",
                column: "HomeId");

            migrationBuilder.AddForeignKey(
                name: "FK_FamilyTrees_Home_Alpha_HomeId",
                table: "FamilyTrees",
                column: "HomeId",
                principalTable: "Home_Alpha",
                principalColumn: "HomeId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FamilyTrees_Home_Alpha_HomeId",
                table: "FamilyTrees");

            migrationBuilder.DropTable(
                name: "Home_Alpha");

            migrationBuilder.DropIndex(
                name: "IX_FamilyTrees_HomeId",
                table: "FamilyTrees");

            migrationBuilder.DropColumn(
                name: "HomeId",
                table: "FamilyTrees");
        }
    }
}

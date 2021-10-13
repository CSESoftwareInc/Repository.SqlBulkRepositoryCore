using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CSESoftware.Repository.SqlBulkRepositoryCore.TestDatabase.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FamilyTrees",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsAlive = table.Column<bool>(type: "bit", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Birthdate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FatherId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MotherId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyTrees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FamilyTrees_FamilyTrees_FatherId",
                        column: x => x.FatherId,
                        principalTable: "FamilyTrees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FamilyTrees_FamilyTrees_MotherId",
                        column: x => x.MotherId,
                        principalTable: "FamilyTrees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FamilyTreeLink",
                columns: table => new
                {
                    PrimarySiblingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SecondarySiblingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyTreeLink", x => new { x.PrimarySiblingId, x.SecondarySiblingId });
                    table.ForeignKey(
                        name: "FK_FamilyTreeLink_FamilyTrees_PrimarySiblingId",
                        column: x => x.PrimarySiblingId,
                        principalTable: "FamilyTrees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FamilyTreeLink_FamilyTrees_SecondarySiblingId",
                        column: x => x.SecondarySiblingId,
                        principalTable: "FamilyTrees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FamilyTreeLink_SecondarySiblingId",
                table: "FamilyTreeLink",
                column: "SecondarySiblingId");

            migrationBuilder.CreateIndex(
                name: "IX_FamilyTrees_FatherId",
                table: "FamilyTrees",
                column: "FatherId");

            migrationBuilder.CreateIndex(
                name: "IX_FamilyTrees_MotherId",
                table: "FamilyTrees",
                column: "MotherId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FamilyTreeLink");

            migrationBuilder.DropTable(
                name: "FamilyTrees");
        }
    }
}

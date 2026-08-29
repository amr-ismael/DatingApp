using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DatingApp.API.Migrations
{
    /// <inheritdoc />
    public partial class AddMatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Matches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LowerUserId = table.Column<int>(type: "int", nullable: false),
                    HigherUserId = table.Column<int>(type: "int", nullable: false),
                    MatchedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    UnmatchedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Matches", x => x.Id);
                    table.CheckConstraint("CK_Match_OrderedIds", "[LowerUserId] < [HigherUserId]");
                    table.ForeignKey(
                        name: "FK_Matches_Users_HigherUserId",
                        column: x => x.HigherUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Matches_Users_LowerUserId",
                        column: x => x.LowerUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Matches_HigherUserId",
                table: "Matches",
                column: "HigherUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_LowerUserId",
                table: "Matches",
                column: "LowerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_LowerUserId_HigherUserId",
                table: "Matches",
                columns: new[] { "LowerUserId", "HigherUserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Matches");
        }
    }
}

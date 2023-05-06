using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace bangsoo.Migrations
{
    /// <inheritdoc />
    public partial class test5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Boards_BoardId",
                table: "Comments");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Boards_BoardId",
                table: "Comments",
                column: "BoardId",
                principalTable: "Boards",
                principalColumn: "BoardId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Boards_BoardId",
                table: "Comments");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Boards_BoardId",
                table: "Comments",
                column: "BoardId",
                principalTable: "Boards",
                principalColumn: "BoardId");
        }
    }
}

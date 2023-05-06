using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace bangsoo.Migrations
{
    /// <inheritdoc />
    public partial class test4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Boards_Users_Id",
                table: "Boards");

            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Users_Id",
                table: "Comments");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Comments",
                newName: "NickName");

            migrationBuilder.RenameIndex(
                name: "IX_Comments_Id",
                table: "Comments",
                newName: "IX_Comments_NickName");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Boards",
                newName: "NickName");

            migrationBuilder.RenameIndex(
                name: "IX_Boards_Id",
                table: "Boards",
                newName: "IX_Boards_NickName");

            migrationBuilder.AddForeignKey(
                name: "FK_Boards_Users_NickName",
                table: "Boards",
                column: "NickName",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Users_NickName",
                table: "Comments",
                column: "NickName",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Boards_Users_NickName",
                table: "Boards");

            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Users_NickName",
                table: "Comments");

            migrationBuilder.RenameColumn(
                name: "NickName",
                table: "Comments",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_Comments_NickName",
                table: "Comments",
                newName: "IX_Comments_Id");

            migrationBuilder.RenameColumn(
                name: "NickName",
                table: "Boards",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_Boards_NickName",
                table: "Boards",
                newName: "IX_Boards_Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Boards_Users_Id",
                table: "Boards",
                column: "Id",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Users_Id",
                table: "Comments",
                column: "Id",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}

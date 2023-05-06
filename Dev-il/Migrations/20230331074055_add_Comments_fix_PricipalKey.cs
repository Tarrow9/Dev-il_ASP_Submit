using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace bangsoo.Migrations
{
    /// <inheritdoc />
    public partial class add_Comments_fix_PricipalKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Boards_Users_NickName",
                table: "Boards");

            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Users_NickName",
                table: "Comments");

            migrationBuilder.AlterColumn<string>(
                name: "NickName",
                table: "Comments",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450);

            migrationBuilder.AlterColumn<string>(
                name: "NickName",
                table: "Boards",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Users_NickName",
                table: "Users",
                column: "NickName");

            migrationBuilder.AddForeignKey(
                name: "FK_Boards_Users_NickName",
                table: "Boards",
                column: "NickName",
                principalTable: "Users",
                principalColumn: "NickName");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Users_NickName",
                table: "Comments",
                column: "NickName",
                principalTable: "Users",
                principalColumn: "NickName");
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

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Users_NickName",
                table: "Users");

            migrationBuilder.AlterColumn<string>(
                name: "NickName",
                table: "Comments",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "NickName",
                table: "Boards",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

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
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace bangsoo.Migrations
{
    /// <inheritdoc />
    public partial class add_Comments_reply : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Depth",
                table: "Comments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ReplyCid",
                table: "Comments",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Depth",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "ReplyCid",
                table: "Comments");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatApp.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddedColumnForFileType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "Messages",
                newName: "FileType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FileType",
                table: "Messages",
                newName: "ImageUrl");
        }
    }
}

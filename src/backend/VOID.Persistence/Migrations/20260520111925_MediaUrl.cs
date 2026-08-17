using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VOID.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MediaUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "Messages",
                newName: "MediaUrl");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MediaUrl",
                table: "Messages",
                newName: "ImageUrl");
        }
    }
}

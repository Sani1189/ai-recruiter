using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Recruiter.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Rename_Field_in_File_Model_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FolderName",
                table: "Files",
                newName: "FolderPath");

            migrationBuilder.AlterColumn<string>(
                name: "StorageAccountName",
                table: "Files",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Container",
                table: "Files",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.CreateIndex(
                name: "IX_Files_Container_FolderPath",
                table: "Files",
                columns: new[] { "Container", "FolderPath" });

            migrationBuilder.CreateIndex(
                name: "IX_Files_FolderPath",
                table: "Files",
                column: "FolderPath");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Files_Container_FolderPath",
                table: "Files");

            migrationBuilder.DropIndex(
                name: "IX_Files_FolderPath",
                table: "Files");

            migrationBuilder.RenameColumn(
                name: "FolderPath",
                table: "Files",
                newName: "FolderName");

            migrationBuilder.AlterColumn<string>(
                name: "StorageAccountName",
                table: "Files",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "Container",
                table: "Files",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);
        }
    }
}

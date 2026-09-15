using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LostAndFoundAgency.Migrations
{
    /// <inheritdoc />
    public partial class AddDatesAndStorageFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LostDate",
                table: "LostRequests",
                newName: "DateLost");

            migrationBuilder.RenameColumn(
                name: "StoragePlace",
                table: "FoundItems",
                newName: "StorageLocation");

            migrationBuilder.RenameColumn(
                name: "FoundDate",
                table: "FoundItems",
                newName: "DateFound");

            migrationBuilder.AddColumn<string>(
                name: "Login",
                table: "Persons",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "Persons",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Login",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "Persons");

            migrationBuilder.RenameColumn(
                name: "DateLost",
                table: "LostRequests",
                newName: "LostDate");

            migrationBuilder.RenameColumn(
                name: "StorageLocation",
                table: "FoundItems",
                newName: "StoragePlace");

            migrationBuilder.RenameColumn(
                name: "DateFound",
                table: "FoundItems",
                newName: "FoundDate");
        }
    }
}

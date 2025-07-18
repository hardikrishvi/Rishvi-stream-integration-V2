using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rishvi.Migrations
{
    /// <inheritdoc />
    public partial class change_auth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DefaultLocation",
                table: "Authorizations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "UseDefaultLocation",
                table: "Authorizations",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefaultLocation",
                table: "Authorizations");

            migrationBuilder.DropColumn(
                name: "UseDefaultLocation",
                table: "Authorizations");
        }
    }
}

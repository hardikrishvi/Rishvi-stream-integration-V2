using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rishvi.Migrations
{
    /// <inheritdoc />
    public partial class change_auth1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LinnHour",
                table: "Authorizations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LinnPage",
                table: "Authorizations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "SendChangeToStream",
                table: "Authorizations",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LinnHour",
                table: "Authorizations");

            migrationBuilder.DropColumn(
                name: "LinnPage",
                table: "Authorizations");

            migrationBuilder.DropColumn(
                name: "SendChangeToStream",
                table: "Authorizations");
        }
    }
}

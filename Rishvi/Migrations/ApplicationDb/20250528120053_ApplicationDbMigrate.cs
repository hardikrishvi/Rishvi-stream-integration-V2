using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rishvi.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class ApplicationDbMigrate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    event_code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    event_code_desc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    event_desc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    event_date = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    event_time = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    event_text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    event_link = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Event", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "WebhookOrders",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    sequence = table.Column<int>(type: "int", nullable: false),
                    order = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Order", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Runs",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    loadId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Run", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Subscriptions",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    party_id = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    @event = table.Column<string>(name: "event", type: "nvarchar(max)", nullable: false),
                    event_type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    url_path = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    http_method = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscription", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Events");

            migrationBuilder.DropTable(
                name: "WebhookOrders");

            migrationBuilder.DropTable(
                name: "Runs");

            migrationBuilder.DropTable(
                name: "Subscriptions");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthService.Infrastructure.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class RemoveProfileFromAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "display_name",
                schema: "auth",
                table: "accounts");

            migrationBuilder.DropColumn(
                name: "profile",
                schema: "auth",
                table: "accounts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "display_name",
                schema: "auth",
                table: "accounts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "profile",
                schema: "auth",
                table: "accounts",
                type: "jsonb",
                nullable: false,
                defaultValue: "{}");
        }
    }
}

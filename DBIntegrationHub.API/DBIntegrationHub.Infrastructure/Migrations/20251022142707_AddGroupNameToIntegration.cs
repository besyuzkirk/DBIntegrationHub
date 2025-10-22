using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DBIntegrationHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGroupNameToIntegration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ExecutionOrder",
                table: "Integrations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "GroupName",
                table: "Integrations",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Integrations_GroupName",
                table: "Integrations",
                column: "GroupName");

            migrationBuilder.CreateIndex(
                name: "IX_Integrations_GroupName_ExecutionOrder",
                table: "Integrations",
                columns: new[] { "GroupName", "ExecutionOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Integrations_GroupName",
                table: "Integrations");

            migrationBuilder.DropIndex(
                name: "IX_Integrations_GroupName_ExecutionOrder",
                table: "Integrations");

            migrationBuilder.DropColumn(
                name: "ExecutionOrder",
                table: "Integrations");

            migrationBuilder.DropColumn(
                name: "GroupName",
                table: "Integrations");
        }
    }
}

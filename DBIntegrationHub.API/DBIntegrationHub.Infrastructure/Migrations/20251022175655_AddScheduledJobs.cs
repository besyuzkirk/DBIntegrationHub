using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DBIntegrationHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddScheduledJobs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ScheduledJobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CronExpression = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IntegrationId = table.Column<Guid>(type: "uuid", nullable: true),
                    GroupId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastRunAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NextRunAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TotalRuns = table.Column<int>(type: "integer", nullable: false),
                    SuccessfulRuns = table.Column<int>(type: "integer", nullable: false),
                    FailedRuns = table.Column<int>(type: "integer", nullable: false),
                    HangfireJobId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduledJobs_Integrations_IntegrationId",
                        column: x => x.IntegrationId,
                        principalTable: "Integrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledJobs_GroupId",
                table: "ScheduledJobs",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledJobs_HangfireJobId",
                table: "ScheduledJobs",
                column: "HangfireJobId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledJobs_IntegrationId",
                table: "ScheduledJobs",
                column: "IntegrationId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledJobs_IsActive",
                table: "ScheduledJobs",
                column: "IsActive");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScheduledJobs");
        }
    }
}

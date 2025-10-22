using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DBIntegrationHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIntegrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Integrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SourceConnectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetConnectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceQuery = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                    TargetQuery = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Integrations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Integrations_Connections_SourceConnectionId",
                        column: x => x.SourceConnectionId,
                        principalTable: "Connections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Integrations_Connections_TargetConnectionId",
                        column: x => x.TargetConnectionId,
                        principalTable: "Connections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Integrations_Name",
                table: "Integrations",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Integrations_SourceConnectionId",
                table: "Integrations",
                column: "SourceConnectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Integrations_TargetConnectionId",
                table: "Integrations",
                column: "TargetConnectionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Integrations");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CommunicationInterne.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AjouterVoiesCommunication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VoiesCommunication",
                schema: "communication_interne",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AgentIdRh = table.Column<int>(type: "integer", nullable: false),
                    Matricule = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoiesCommunication", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VoiesEmail",
                schema: "communication_interne",
                columns: table => new
                {
                    VoieCommunicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Adresse = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoiesEmail", x => new { x.VoieCommunicationId, x.Type });
                    table.ForeignKey(
                        name: "FK_VoiesEmail_VoiesCommunication_VoieCommunicationId",
                        column: x => x.VoieCommunicationId,
                        principalSchema: "communication_interne",
                        principalTable: "VoiesCommunication",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VoiesTelephone",
                schema: "communication_interne",
                columns: table => new
                {
                    VoieCommunicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Numero = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoiesTelephone", x => new { x.VoieCommunicationId, x.Type });
                    table.ForeignKey(
                        name: "FK_VoiesTelephone_VoiesCommunication_VoieCommunicationId",
                        column: x => x.VoieCommunicationId,
                        principalSchema: "communication_interne",
                        principalTable: "VoiesCommunication",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VoiesCommunication_AgentIdRh",
                schema: "communication_interne",
                table: "VoiesCommunication",
                column: "AgentIdRh",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VoiesCommunication_Matricule",
                schema: "communication_interne",
                table: "VoiesCommunication",
                column: "Matricule");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VoiesEmail",
                schema: "communication_interne");

            migrationBuilder.DropTable(
                name: "VoiesTelephone",
                schema: "communication_interne");

            migrationBuilder.DropTable(
                name: "VoiesCommunication",
                schema: "communication_interne");
        }
    }
}

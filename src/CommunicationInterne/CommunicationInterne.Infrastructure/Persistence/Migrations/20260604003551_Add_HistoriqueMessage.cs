using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CommunicationInterne.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Add_HistoriqueMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NumeroVersion",
                schema: "communication_interne",
                table: "Messages",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "HistoriquesMessage",
                schema: "communication_interne",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MessageInterneId = table.Column<Guid>(type: "uuid", nullable: false),
                    NumeroVersion = table.Column<int>(type: "integer", nullable: false),
                    Objet = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Corps = table.Column<string>(type: "text", nullable: false),
                    EstInstitutionnel = table.Column<bool>(type: "boolean", nullable: false),
                    Canaux = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ModifiePar = table.Column<Guid>(type: "uuid", nullable: false),
                    DateModification = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoriquesMessage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistoriquesMessage_Messages_MessageInterneId",
                        column: x => x.MessageInterneId,
                        principalSchema: "communication_interne",
                        principalTable: "Messages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HistoriquesMessage_MessageInterneId",
                schema: "communication_interne",
                table: "HistoriquesMessage",
                column: "MessageInterneId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoriquesMessage_MessageInterneId_NumeroVersion",
                schema: "communication_interne",
                table: "HistoriquesMessage",
                columns: new[] { "MessageInterneId", "NumeroVersion" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HistoriquesMessage",
                schema: "communication_interne");

            migrationBuilder.DropColumn(
                name: "NumeroVersion",
                schema: "communication_interne",
                table: "Messages");
        }
    }
}

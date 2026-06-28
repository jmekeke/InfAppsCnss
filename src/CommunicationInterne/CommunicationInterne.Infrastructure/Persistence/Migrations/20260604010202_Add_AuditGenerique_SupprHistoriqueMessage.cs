using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CommunicationInterne.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Add_AuditGenerique_SupprHistoriqueMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HistoriquesMessage",
                schema: "communication_interne");

            migrationBuilder.DropColumn(
                name: "NumeroVersion",
                schema: "communication_interne",
                table: "Messages");

            migrationBuilder.CreateTable(
                name: "historique_actions",
                schema: "communication_interne",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EntiteType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    EntiteId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    EtatAvant = table.Column<string>(type: "text", nullable: true),
                    EtatApres = table.Column<string>(type: "text", nullable: true),
                    UtilisateurId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    UtilisateurNom = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    UtilisateurRole = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    DestinataireId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    DestinataireNom = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    DateAction = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Commentaire = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_historique_actions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_historique_actions_DateAction",
                schema: "communication_interne",
                table: "historique_actions",
                column: "DateAction");

            migrationBuilder.CreateIndex(
                name: "IX_historique_actions_EntiteType_EntiteId",
                schema: "communication_interne",
                table: "historique_actions",
                columns: new[] { "EntiteType", "EntiteId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "historique_actions",
                schema: "communication_interne");

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
                    Canaux = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Corps = table.Column<string>(type: "text", nullable: false),
                    DateModification = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EstInstitutionnel = table.Column<bool>(type: "boolean", nullable: false),
                    MessageInterneId = table.Column<Guid>(type: "uuid", nullable: false),
                    ModifiePar = table.Column<Guid>(type: "uuid", nullable: false),
                    NumeroVersion = table.Column<int>(type: "integer", nullable: false),
                    Objet = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
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
    }
}

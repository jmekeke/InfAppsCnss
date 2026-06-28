using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CommunicationInterne.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Init_CommunicationInterne : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "communication_interne");

            migrationBuilder.CreateTable(
                name: "DossiersDiffusion",
                schema: "communication_interne",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MessageInterneId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeclencheurId = table.Column<Guid>(type: "uuid", nullable: false),
                    DateLancement = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NombreDestinatairesTotaux = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DossiersDiffusion", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Groupes",
                schema: "communication_interne",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nom = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    TypeGroupe = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreateurId = table.Column<Guid>(type: "uuid", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EstActif = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groupes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                schema: "communication_interne",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Objet = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Corps = table.Column<string>(type: "text", nullable: false),
                    EstInstitutionnel = table.Column<bool>(type: "boolean", nullable: false),
                    AuteurId = table.Column<Guid>(type: "uuid", nullable: false),
                    Statut = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateValidation = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ValidateurId = table.Column<Guid>(type: "uuid", nullable: true),
                    MotiDeRejet = table.Column<string>(type: "text", nullable: true),
                    DateProgrammee = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateDiffusion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EstArchive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LignesDiffusion",
                schema: "communication_interne",
                columns: table => new
                {
                    DossierDiffusionId = table.Column<Guid>(type: "uuid", nullable: false),
                    AgentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Canal = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Statut = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DateEnvoi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MessageErreur = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LignesDiffusion", x => new { x.DossierDiffusionId, x.AgentId, x.Canal });
                    table.ForeignKey(
                        name: "FK_LignesDiffusion_DossiersDiffusion_DossierDiffusionId",
                        column: x => x.DossierDiffusionId,
                        principalSchema: "communication_interne",
                        principalTable: "DossiersDiffusion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MembresGroupe",
                schema: "communication_interne",
                columns: table => new
                {
                    GroupeDiffusionId = table.Column<Guid>(type: "uuid", nullable: false),
                    AgentId = table.Column<Guid>(type: "uuid", nullable: false),
                    DateAjout = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MembresGroupe", x => new { x.GroupeDiffusionId, x.AgentId });
                    table.ForeignKey(
                        name: "FK_MembresGroupe_Groupes_GroupeDiffusionId",
                        column: x => x.GroupeDiffusionId,
                        principalSchema: "communication_interne",
                        principalTable: "Groupes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MessageCanaux",
                schema: "communication_interne",
                columns: table => new
                {
                    MessageInterneId = table.Column<Guid>(type: "uuid", nullable: false),
                    Canal = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageCanaux", x => new { x.MessageInterneId, x.Canal });
                    table.ForeignKey(
                        name: "FK_MessageCanaux_Messages_MessageInterneId",
                        column: x => x.MessageInterneId,
                        principalSchema: "communication_interne",
                        principalTable: "Messages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MessageGroupesDestinataires",
                schema: "communication_interne",
                columns: table => new
                {
                    MessageInterneId = table.Column<Guid>(type: "uuid", nullable: false),
                    GroupeId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageGroupesDestinataires", x => new { x.MessageInterneId, x.GroupeId });
                    table.ForeignKey(
                        name: "FK_MessageGroupesDestinataires_Messages_MessageInterneId",
                        column: x => x.MessageInterneId,
                        principalSchema: "communication_interne",
                        principalTable: "Messages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PiecesJointes",
                schema: "communication_interne",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MessageInterneId = table.Column<Guid>(type: "uuid", nullable: false),
                    NomFichier = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    TypeMime = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TailleOctets = table.Column<long>(type: "bigint", nullable: false),
                    DateAjout = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PiecesJointes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PiecesJointes_Messages_MessageInterneId",
                        column: x => x.MessageInterneId,
                        principalSchema: "communication_interne",
                        principalTable: "Messages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DossiersDiffusion_DateLancement",
                schema: "communication_interne",
                table: "DossiersDiffusion",
                column: "DateLancement");

            migrationBuilder.CreateIndex(
                name: "IX_DossiersDiffusion_MessageInterneId",
                schema: "communication_interne",
                table: "DossiersDiffusion",
                column: "MessageInterneId");

            migrationBuilder.CreateIndex(
                name: "IX_Groupes_CreateurId",
                schema: "communication_interne",
                table: "Groupes",
                column: "CreateurId");

            migrationBuilder.CreateIndex(
                name: "IX_Groupes_Nom",
                schema: "communication_interne",
                table: "Groupes",
                column: "Nom");

            migrationBuilder.CreateIndex(
                name: "IX_LignesDiffusion_AgentId",
                schema: "communication_interne",
                table: "LignesDiffusion",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_LignesDiffusion_Statut",
                schema: "communication_interne",
                table: "LignesDiffusion",
                column: "Statut");

            migrationBuilder.CreateIndex(
                name: "IX_MembresGroupe_AgentId",
                schema: "communication_interne",
                table: "MembresGroupe",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageGroupesDestinataires_GroupeId",
                schema: "communication_interne",
                table: "MessageGroupesDestinataires",
                column: "GroupeId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_AuteurId",
                schema: "communication_interne",
                table: "Messages",
                column: "AuteurId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_Statut",
                schema: "communication_interne",
                table: "Messages",
                column: "Statut");

            migrationBuilder.CreateIndex(
                name: "IX_PiecesJointes_MessageInterneId",
                schema: "communication_interne",
                table: "PiecesJointes",
                column: "MessageInterneId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LignesDiffusion",
                schema: "communication_interne");

            migrationBuilder.DropTable(
                name: "MembresGroupe",
                schema: "communication_interne");

            migrationBuilder.DropTable(
                name: "MessageCanaux",
                schema: "communication_interne");

            migrationBuilder.DropTable(
                name: "MessageGroupesDestinataires",
                schema: "communication_interne");

            migrationBuilder.DropTable(
                name: "PiecesJointes",
                schema: "communication_interne");

            migrationBuilder.DropTable(
                name: "DossiersDiffusion",
                schema: "communication_interne");

            migrationBuilder.DropTable(
                name: "Groupes",
                schema: "communication_interne");

            migrationBuilder.DropTable(
                name: "Messages",
                schema: "communication_interne");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CommunicationInterne.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AjouterHistoriqueVoie : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateModification",
                schema: "communication_interne",
                table: "VoiesTelephone",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "EstActif",
                schema: "communication_interne",
                table: "VoiesTelephone",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateModification",
                schema: "communication_interne",
                table: "VoiesEmail",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "EstActif",
                schema: "communication_interne",
                table: "VoiesEmail",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateTable(
                name: "HistoriquesVoie",
                schema: "communication_interne",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    VoieCommunicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Canal = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    TypeVoie = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Valeur = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    EstActif = table.Column<bool>(type: "boolean", nullable: false),
                    Action = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ModifiePar = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DateAction = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoriquesVoie", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistoriquesVoie_VoiesCommunication_VoieCommunicationId",
                        column: x => x.VoieCommunicationId,
                        principalSchema: "communication_interne",
                        principalTable: "VoiesCommunication",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HistoriquesVoie_Canal",
                schema: "communication_interne",
                table: "HistoriquesVoie",
                column: "Canal");

            migrationBuilder.CreateIndex(
                name: "IX_HistoriquesVoie_VoieCommunicationId_DateAction",
                schema: "communication_interne",
                table: "HistoriquesVoie",
                columns: new[] { "VoieCommunicationId", "DateAction" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HistoriquesVoie",
                schema: "communication_interne");

            migrationBuilder.DropColumn(
                name: "DateModification",
                schema: "communication_interne",
                table: "VoiesTelephone");

            migrationBuilder.DropColumn(
                name: "EstActif",
                schema: "communication_interne",
                table: "VoiesTelephone");

            migrationBuilder.DropColumn(
                name: "DateModification",
                schema: "communication_interne",
                table: "VoiesEmail");

            migrationBuilder.DropColumn(
                name: "EstActif",
                schema: "communication_interne",
                table: "VoiesEmail");
        }
    }
}

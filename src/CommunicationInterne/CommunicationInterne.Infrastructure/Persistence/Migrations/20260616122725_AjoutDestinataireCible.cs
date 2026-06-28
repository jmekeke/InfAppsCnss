using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CommunicationInterne.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AjoutDestinataireCible : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MessageDestinataires",
                schema: "communication_interne",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MessageInterneId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ReferenceId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Libelle = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageDestinataires", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MessageDestinataires_Messages_MessageInterneId",
                        column: x => x.MessageInterneId,
                        principalSchema: "communication_interne",
                        principalTable: "Messages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MessageDestinataires_MessageInterneId",
                schema: "communication_interne",
                table: "MessageDestinataires",
                column: "MessageInterneId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MessageDestinataires",
                schema: "communication_interne");
        }
    }
}

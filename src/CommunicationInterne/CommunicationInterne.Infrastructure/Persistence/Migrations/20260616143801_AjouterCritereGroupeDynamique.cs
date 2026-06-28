using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CommunicationInterne.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AjouterCritereGroupeDynamique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CritereType",
                schema: "communication_interne",
                table: "Groupes",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CritereValeur",
                schema: "communication_interne",
                table: "Groupes",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CritereType",
                schema: "communication_interne",
                table: "Groupes");

            migrationBuilder.DropColumn(
                name: "CritereValeur",
                schema: "communication_interne",
                table: "Groupes");
        }
    }
}

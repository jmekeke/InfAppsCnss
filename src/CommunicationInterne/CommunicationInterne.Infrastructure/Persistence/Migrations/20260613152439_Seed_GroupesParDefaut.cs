using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CommunicationInterne.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Seed_GroupesParDefaut : Migration
    {
        // Guid système utilisé comme CreateurId pour les groupes créés par initialisation.
        private static readonly Guid SystemId = new("00000000-0000-0000-0000-000000000001");

        private static readonly DateTime SeedDate = new(2026, 6, 13, 0, 0, 0, DateTimeKind.Utc);

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var groupes = new[]
            {
                (Id: new Guid("a1000000-0000-0000-0000-000000000001"), Nom: "Haut Cadre Direction Centrale", Description: "Groupe des hauts cadres de la Direction Centrale"),
                (Id: new Guid("a1000000-0000-0000-0000-000000000002"), Nom: "Haut Cadre CG",                 Description: "Groupe des hauts cadres du Conseil de Gestion"),
                (Id: new Guid("a1000000-0000-0000-0000-000000000003"), Nom: "Haut Cadre CNSS",               Description: "Groupe des hauts cadres de la CNSS"),
                (Id: new Guid("a1000000-0000-0000-0000-000000000004"), Nom: "Cadres Moyens",                 Description: "Groupe des cadres moyens"),
                (Id: new Guid("a1000000-0000-0000-0000-000000000005"), Nom: "Agent Maitrise",                Description: "Groupe des agents de maîtrise"),
                (Id: new Guid("a1000000-0000-0000-0000-000000000006"), Nom: "Agent Classifié",               Description: "Groupe des agents classifiés"),
            };

            foreach (var g in groupes)
            {
                migrationBuilder.InsertData(
                    schema: "communication_interne",
                    table: "Groupes",
                    columns: ["Id", "Nom", "Description", "TypeGroupe", "CreateurId", "DateCreation", "EstActif"],
                    values: [g.Id, g.Nom, g.Description, "Manuel", SystemId, SeedDate, true]);
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var ids = new[]
            {
                new Guid("a1000000-0000-0000-0000-000000000001"),
                new Guid("a1000000-0000-0000-0000-000000000002"),
                new Guid("a1000000-0000-0000-0000-000000000003"),
                new Guid("a1000000-0000-0000-0000-000000000004"),
                new Guid("a1000000-0000-0000-0000-000000000005"),
                new Guid("a1000000-0000-0000-0000-000000000006"),
            };

            foreach (var id in ids)
            {
                migrationBuilder.DeleteData(
                    schema: "communication_interne",
                    table: "Groupes",
                    keyColumn: "Id",
                    keyValue: id);
            }
        }
    }
}

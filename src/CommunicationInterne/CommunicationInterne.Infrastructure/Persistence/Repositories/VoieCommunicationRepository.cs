using Cnss.Metier.CommunicationInterne.Domain.Aggregats;
using Cnss.Metier.CommunicationInterne.Domain.Repositories;
using Cnss.Metier.CommunicationInterne.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Cnss.Metier.CommunicationInterne.Infrastructure.Persistence.Repositories;

internal sealed class VoieCommunicationRepository(CommunicationInterneDbContext db)
    : IVoieCommunicationRepository
{
    public Task<VoieCommunication?> GetByAgentIdRhAsync(int agentIdRh, CancellationToken ct = default)
        => db.VoiesCommunication
             .Include(v => v.Telephones)
             .Include(v => v.Emails)
             .Include(v => v.Historique)
             .FirstOrDefaultAsync(v => v.AgentIdRh == agentIdRh, ct);

    public void Add(VoieCommunication voie) => db.VoiesCommunication.Add(voie);

    public async Task<List<(int AgentIdRh, string? TelephoneActif, string? EmailProfActif)>> GetResumesAsync(CancellationToken ct = default)
    {
        // On projette côté base : seulement AgentIdRh + voies actives (sans charger l'agrégat entier)
        var data = await db.VoiesCommunication
            .Select(v => new
            {
                v.AgentIdRh,
                Telephones = v.Telephones.Where(t => t.EstActif).Select(t => new { t.Type, t.Numero }).ToList(),
                Emails     = v.Emails.Where(e => e.EstActif).Select(e => new { e.Type, e.Adresse }).ToList(),
            })
            .ToListAsync(ct);

        // OrderBy sur les enums fonctionne en mémoire (valeurs int : Appel=1 < Sms=2 < WhatsApp=3)
        return data.Select(v => (
            v.AgentIdRh,
            v.Telephones.OrderBy(t => t.Type).Select(t => t.Numero).FirstOrDefault(),
            v.Emails.OrderBy(e => e.Type).Select(e => e.Adresse).FirstOrDefault()
        )).ToList();
    }
}

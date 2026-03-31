using Microsoft.EntityFrameworkCore;
using SportsLeague.DataAccess.Context;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Interfaces.Repositories;

namespace SportsLeague.DataAccess.Repositories;

public class SponsorRepository : GenericRepository<Sponsor>, ISponsorRepository
{
    public SponsorRepository(LeagueDbContext context) : base(context)
    {
    }

    public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null)
    {
        var query = _dbSet.AsQueryable();

        if (excludeId.HasValue)
            query = query.Where(s => s.Id != excludeId.Value);

        return await query.AnyAsync(s => s.Name.ToLower() == name.ToLower());
    }

    public async Task<Sponsor?> GetSponsorWithTournamentsAsync(int id)
    {
        return await _dbSet
            .Include(s => s.TournamentSponsors)
                .ThenInclude(ts => ts.Tournament)
            .FirstOrDefaultAsync(s => s.Id == id);
    }
}
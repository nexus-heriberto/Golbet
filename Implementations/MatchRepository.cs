using Golbet.Data;
using Golbet.Entities;
using Golbet.Enums;
using Golbet.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Golbet.Implementations;

public class MatchRepository : GenericRepository<Match>, IMatchRepository
{
    public MatchRepository(AppDbContext context)
        : base(context)
    {
    }

    public async Task<IEnumerable<Match>> GetAllWithTeamsAsync(
        MatchStatus? status = null,
        string? search = null)
    {
        var query = _dbSet
            .Include(m => m.HomeTeam)
            .Include(m => m.AwayTeam)
            .Where(m => m.IsActive)
            .AsNoTracking()
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(m => m.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = search.Trim();

            query = query.Where(m =>
                EF.Functions.Like(m.HomeTeam.Name, $"%{searchTerm}%") ||
                EF.Functions.Like(m.AwayTeam.Name, $"%{searchTerm}%"));
        }

        return await query
            .OrderBy(m => m.Date)
            .ToListAsync();
    }

    public async Task<Match?> GetByIdWithDetailsAsync(int id)
        => await _dbSet
            .Include(m => m.HomeTeam)
            .Include(m => m.AwayTeam)
            .Include(m => m.Bets)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);
}

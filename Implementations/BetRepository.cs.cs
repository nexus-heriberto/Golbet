using Golbet.Data;
using Golbet.Entities;
using Golbet.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Golbet.Implementations;

public class BetRepository : GenericRepository<Bet>, IBetRepository
{
    public BetRepository(AppDbContext context)
        : base(context)
    {
    }

    public async Task<IEnumerable<Bet>> GetByUserAsync(string userId)
    {
        return await _dbSet
            .Where(b => b.UserId == userId)
            .Include(b => b.Match)
                .ThenInclude(m => m.HomeTeam)
            .Include(b => b.Match)
                .ThenInclude(m => m.AwayTeam)
            .OrderByDescending(b => b.CreatedDate)
            .AsNoTracking()
            .ToListAsync();
    }
}
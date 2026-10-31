using Golbet.Entities;

namespace Golbet.Interfaces;

public interface IBetRepository : IGenericRepository<Bet>
{
    Task<IEnumerable<Bet>> GetByUserAsync(string userId);
}

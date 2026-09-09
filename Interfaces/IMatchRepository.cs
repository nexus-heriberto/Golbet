using Golbet.Entities;
using Golbet.Enums;

namespace Golbet.Interfaces;

public interface IMatchRepository : IGenericRepository<Match>
{
    Task<IEnumerable<Match>> GetAllWithTeamsAsync(
        MatchStatus? status = null);

    Task<Match?> GetByIdWithDetailsAsync(int id);
}
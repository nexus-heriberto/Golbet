
using Golbet.Entities;

namespace Golbet.Interfaces;

public interface IBetRepository : IGenericRepository<Bet>
{
    Task<IEnumerable<Bet>> GetByUserAsync(string userId);

    /// <summary>
    /// Obtiene las apuestas pendientes de un partido,
    /// rastreadas y con sus usuarios cargados.
    /// </summary>
    Task<List<Bet>> GetPendingByMatchAsync(int matchId);
}


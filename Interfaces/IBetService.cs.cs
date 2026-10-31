using Golbet.DTOs;

namespace Golbet.Services.Interfaces;

public interface IBetService
{
    Task PlaceBetAsync(string userId, BetFormDto dto);

    Task<IEnumerable<BetDto>> GetMyBetsAsync(string userId);
}

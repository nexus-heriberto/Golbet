using Golbet.DTOs;
using Golbet.Enums;

namespace Golbet.Services.Interfaces;

public interface IMatchService
{
    Task<IEnumerable<MatchDto>> GetBoardAsync(MatchStatus? status = null);
}
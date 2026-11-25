
using Golbet.DTOs;
using Golbet.Enums;
using Golbet.Services.DTOs;

namespace Golbet.Services.Interfaces;

public interface IMatchService
{
    Task<IEnumerable<MatchDto>> GetBoardAsync(
        MatchStatus? status = null,
        string? search = null);

    Task<MatchDetailDto?> GetDetailAsync(int id);

    Task<MatchFormDto?> GetForEditAsync(int id);

    Task CreateAsync(MatchFormDto dto);

    Task UpdateAsync(MatchFormDto dto);

    Task DeactivateAsync(int id);

    // Module 9: Match lifecycle and bet resolution.
    Task StartMatchAsync(int id);

    Task<ResolutionSummaryDto> CloseMatchAsync(MatchResultFormDto dto);
}


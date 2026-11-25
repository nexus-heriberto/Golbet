
using AutoMapper;
using Golbet.DTOs;
using Golbet.Entities;
using Golbet.Enums;
using Golbet.Interfaces;
using Golbet.Services.DTOs;
using Golbet.Services.Helpers;
using Golbet.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Golbet.Services.Implementations;

public class MatchService : IMatchService
{
    private readonly IMatchRepository _matchRepository;
    private readonly IBetRepository _betRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<MatchService> _logger;

    public MatchService(
        IMatchRepository matchRepository,
        IBetRepository betRepository,
        IMapper mapper,
        ILogger<MatchService> logger)
    {
        _matchRepository = matchRepository;
        _betRepository = betRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<MatchDto>> GetBoardAsync(
        MatchStatus? status = null,
        string? search = null)
    {
        var matches = await _matchRepository
            .GetAllWithTeamsAsync(status, search);

        return _mapper.Map<IEnumerable<MatchDto>>(matches);
    }

    public async Task<MatchDetailDto?> GetDetailAsync(int id)
    {
        var match = await _matchRepository
            .GetByIdWithDetailsAsync(id);

        return match is null
            ? null
            : _mapper.Map<MatchDetailDto>(match);
    }

    public async Task<MatchFormDto?> GetForEditAsync(int id)
    {
        var match = await _matchRepository.GetByIdAsync(id);

        if (match is null)
        {
            return null;
        }

        var dto = _mapper.Map<MatchFormDto>(match);
        dto.Date = dto.Date.ToColombiaTime();

        return dto;
    }

    public async Task CreateAsync(MatchFormDto dto)
    {
        ValidateBusinessRules(dto);

        var match = _mapper.Map<Match>(dto);
        match.Date = dto.Date.ToUtcFromColombia();

        await _matchRepository.AddAsync(match);
    }

    public async Task UpdateAsync(MatchFormDto dto)
    {
        ValidateBusinessRules(dto);

        var match = await _matchRepository.GetByIdAsync(dto.Id)
            ?? throw new KeyNotFoundException(
                $"Match {dto.Id} not found.");

        _mapper.Map(dto, match);
        match.Date = dto.Date.ToUtcFromColombia();

        await _matchRepository.UpdateAsync(match);
    }

    public async Task DeactivateAsync(int id)
    {
        await _matchRepository.DeactivateAsync(id);
    }

    // MODULE 9: Start a scheduled match.
    public async Task StartMatchAsync(int id)
    {
        var match = await _matchRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException(
                $"Match {id} not found.");

        if (match.Status != MatchStatus.Scheduled)
        {
            throw new InvalidOperationException(
                "Solo un partido programado puede iniciarse.");
        }

        match.Status = MatchStatus.InProgress;

        await _matchRepository.UpdateAsync(match);
    }

    // MODULE 9: Close the match and resolve pending bets.
    public async Task<ResolutionSummaryDto> CloseMatchAsync(
        MatchResultFormDto dto)
    {
        var match = await _matchRepository.GetByIdAsync(dto.MatchId)
            ?? throw new KeyNotFoundException(
                $"Match {dto.MatchId} not found.");

        if (match.Status == MatchStatus.Finished)
        {
            throw new InvalidOperationException(
                "Este partido ya fue cerrado y sus apuestas resueltas.");
        }

        if (match.Status != MatchStatus.InProgress)
        {
            throw new InvalidOperationException(
                "El partido debe estar En juego para cerrarse. Inícialo primero.");
        }

        if (dto.HomeGoals < 0 || dto.HomeGoals > 99 ||
            dto.AwayGoals < 0 || dto.AwayGoals > 99)
        {
            throw new InvalidOperationException(
                "Los goles deben estar entre 0 y 99.");
        }

        // Register the final score and state.
        match.HomeGoals = dto.HomeGoals;
        match.AwayGoals = dto.AwayGoals;
        match.Status = MatchStatus.Finished;

        var winningPick = GetWinningPick(
            dto.HomeGoals,
            dto.AwayGoals);

        // Load tracked pending bets and their users.
        var pendingBets =
            await _betRepository.GetPendingByMatchAsync(match.Id);

        var summary = new ResolutionSummaryDto
        {
            ResolvedBets = pendingBets.Count
        };

        foreach (var bet in pendingBets)
        {
            if (bet.Pick == winningPick)
            {
                bet.Status = BetStatus.Won;

                // Always use the frozen odds.
                var payout = bet.Amount * bet.OddsAtPlacement;

                bet.User.Balance += payout;

                summary.WonBets++;
                summary.TotalPaid += payout;
            }
            else
            {
                bet.Status = BetStatus.Lost;
            }
        }

        // Save the match, bets and user balances together.
        await _matchRepository.UpdateAsync(match);

        // Log the successful match resolution using structured properties.
        _logger.LogInformation(
            "Match {MatchId} closed {HomeGoals}-{AwayGoals}: " +
            "{ResolvedBets} bets resolved, {WonBets} won, {TotalPaid} paid",
            match.Id,
            dto.HomeGoals,
            dto.AwayGoals,
            summary.ResolvedBets,
            summary.WonBets,
            summary.TotalPaid);

        return summary;
    }

    private static BetPick GetWinningPick(
        int homeGoals,
        int awayGoals)
    {
        return homeGoals > awayGoals
            ? BetPick.Home
            : homeGoals < awayGoals
                ? BetPick.Away
                : BetPick.Draw;
    }

    private static void ValidateBusinessRules(MatchFormDto dto)
    {
        if (dto.HomeTeamId == dto.AwayTeamId)
        {
            throw new InvalidOperationException(
                "El equipo local y el visitante no pueden ser el mismo.");
        }

        if (dto.Date.ToUtcFromColombia() <= DateTime.UtcNow)
        {
            throw new InvalidOperationException(
                "La fecha del partido debe ser futura.");
        }
    }
}

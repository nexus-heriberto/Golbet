using AutoMapper;
using Golbet.DTOs;
using Golbet.Entities;
using Golbet.Enums;
using Golbet.Interfaces;
using Golbet.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Golbet.Services.Implementations;

public class BetService : IBetService
{
    private readonly IBetRepository _betRepository;
    private readonly IMatchRepository _matchRepository;
    private readonly UserManager<AppUser> _userManager;
    private readonly IMapper _mapper;

    public BetService(
        IBetRepository betRepository,
        IMatchRepository matchRepository,
        UserManager<AppUser> userManager,
        IMapper mapper)
    {
        _betRepository = betRepository;
        _matchRepository = matchRepository;
        _userManager = userManager;
        _mapper = mapper;
    }

    public async Task PlaceBetAsync(string userId, BetFormDto dto)
    {
        // Validate the amount.
        if (dto.Amount <= 0)
        {
            throw new InvalidOperationException(
                "El valor de la apuesta debe ser mayor que cero.");
        }

        // Find the match.
        var match = await _matchRepository.GetByIdAsync(dto.MatchId);

        if (match is null || !match.IsActive)
        {
            throw new InvalidOperationException(
                "El partido no existe o no está disponible.");
        }

        // Only future scheduled matches accept bets.
        if (match.Status != MatchStatus.Scheduled ||
            match.Date <= DateTime.UtcNow)
        {
            throw new InvalidOperationException(
                "Solo puedes apostar en partidos programados cuya fecha no haya llegado.");
        }

        // Find the authenticated user.
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
        {
            throw new InvalidOperationException(
                "No se encontró el usuario.");
        }

        // Prevent duplicate picks for the same user and match.
        var userBets = await _betRepository.GetByUserAsync(userId);

        if (userBets.Any(b =>
            b.MatchId == dto.MatchId &&
            b.Pick == dto.Pick))
        {
            throw new InvalidOperationException(
                "Ya tienes una apuesta registrada para esta selección en este partido.");
        }

        // Check the user's balance.
        if (dto.Amount > user.Balance)
        {
            throw new InvalidOperationException(
                "No tienes suficientes FutCoins para realizar esta apuesta.");
        }

        // Get the odds according to the selected outcome.
        var odds = dto.Pick switch
        {
            BetPick.Home => match.HomeOdds,
            BetPick.Draw => match.DrawOdds,
            BetPick.Away => match.AwayOdds,
            _ => throw new InvalidOperationException(
                "La selección de apuesta no es válida.")
        };

        // Deduct the stake.
        user.Balance -= dto.Amount;

        // Create the bet with frozen odds.
        var bet = new Bet
        {
            MatchId = match.Id,
            UserId = userId,
            Pick = dto.Pick,
            Amount = dto.Amount,
            OddsAtPlacement = odds,
            Status = BetStatus.Pending
        };

        // Save the bet and updated balance.
        // Both changes use the same scoped DbContext.
        await _betRepository.AddAsync(bet);
    }

    public async Task<IEnumerable<BetDto>> GetMyBetsAsync(string userId)
    {
        var bets = await _betRepository.GetByUserAsync(userId);

        return _mapper.Map<IEnumerable<BetDto>>(bets);
    }
}

using Golbet.Enums;

namespace Golbet.DTOs;

public class MatchDto
{
    public int Id { get; set; }

    public DateTime Date { get; set; }

    public MatchStatus Status { get; set; }

    public string HomeTeamName { get; set; } = string.Empty;

    public string? HomeTeamCrestUrl { get; set; }

    public string AwayTeamName { get; set; } = string.Empty;

    public string? AwayTeamCrestUrl { get; set; }

    public int? HomeGoals { get; set; }

    public int? AwayGoals { get; set; }

    public decimal HomeOdds { get; set; }

    public decimal DrawOdds { get; set; }

    public decimal AwayOdds { get; set; }
}
using Golbet.Enums;

namespace Golbet.DTOs;

public class BetDto
{
    public int Id { get; set; }

    public string HomeTeamName { get; set; } = string.Empty;

    public string AwayTeamName { get; set; } = string.Empty;

    public DateTime MatchDate { get; set; }

    public BetPick Pick { get; set; }

    public decimal Amount { get; set; }

    public decimal OddsAtPlacement { get; set; }

    public decimal PotentialPayout { get; set; }

    public BetStatus Status { get; set; }

    public DateTime CreatedDate { get; set; }
}

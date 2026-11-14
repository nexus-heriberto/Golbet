namespace Golbet.Services.DTOs;

public class ResolutionSummaryDto
{
    public int ResolvedBets { get; set; }

    public int WonBets { get; set; }

    public decimal TotalPaid { get; set; }
}
using System.ComponentModel.DataAnnotations;
using Golbet.Enums;

namespace Golbet.DTOs;

public class BetFormDto
{
    [Required]
    public int MatchId { get; set; }

    [Required]
    public BetPick Pick { get; set; }

    [Required]
    [Range(
        typeof(decimal),
        "1",
        "9999999999",
        ErrorMessage = "El valor de la apuesta debe estar entre 1 y 9.999.999.999.")]
    public decimal Amount { get; set; }
}
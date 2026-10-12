
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Golbet.Entities;

/// <summary>
/// Usuario de GolBet con información adicional para las apuestas.
/// </summary>
public class AppUser : IdentityUser
{
    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = null!;

    [Column(TypeName = "decimal(12,2)")]
    public decimal Balance { get; set; }

    public ICollection<Bet> Bets { get; set; } = new List<Bet>();
}


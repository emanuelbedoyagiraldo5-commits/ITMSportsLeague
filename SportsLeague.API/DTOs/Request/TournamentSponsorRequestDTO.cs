using System.ComponentModel.DataAnnotations;

namespace SportsLeague.API.DTOs.Request;

public class TournamentSponsorRequestDTO
{
    [Required]
    public int TournamentId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "ContractAmount must be greater than 0")]
    public decimal ContractAmount { get; set; }
}

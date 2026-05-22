using SportsLeague.Domain.Entities;

namespace SportsLeague.Domain.Entities;

public class MatchLineup : AuditBase
{
    public int MatchId { get; set; }
    public int PlayerId { get; set; }
    public bool IsStarter { get; set; }  // true = Titular, false = Suplente
    public string Position { get; set; } = string.Empty;  // "GK", "CB", "CDM", "CAM", "ST", etc.

    // Navigation Properties
    public Match Match { get; set; } = null!;
    public Player Player { get; set; } = null!;
}
using SportsLeague.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace SportsLeague.API.DTOs.Request;

public class SponsorRequestDTO
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string ContactEmail { get; set; } = string.Empty;

    [Phone]
    public string? Phone { get; set; }

    [Url]
    public string? WebsiteUrl { get; set; }

    [Required]
    public SponsorCategory Category { get; set; }
}

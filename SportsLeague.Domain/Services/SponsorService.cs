using Microsoft.Extensions.Logging;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Interfaces.Repositories;
using SportsLeague.Domain.Interfaces.Services;
using System.Text.RegularExpressions;

namespace SportsLeague.Domain.Services;

public class SponsorService : ISponsorService
{
    private readonly ISponsorRepository _sponsorRepository;
    private readonly ITournamentSponsorRepository _tournamentSponsorRepository;
    private readonly ITournamentRepository _tournamentRepository;
    private readonly ILogger<SponsorService> _logger;

    public SponsorService(
        ISponsorRepository sponsorRepository,
        ITournamentSponsorRepository tournamentSponsorRepository,
        ITournamentRepository tournamentRepository,
        ILogger<SponsorService> logger)
    {
        _sponsorRepository = sponsorRepository;
        _tournamentSponsorRepository = tournamentSponsorRepository;
        _tournamentRepository = tournamentRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<Sponsor>> GetAllAsync()
    {
        _logger.LogInformation("Retrieving all sponsors");
        return await _sponsorRepository.GetAllAsync();
    }

    public async Task<Sponsor?> GetByIdAsync(int id)
    {
        _logger.LogInformation("Retrieving sponsor with ID: {SponsorId}", id);
        var sponsor = await _sponsorRepository.GetSponsorWithTournamentsAsync(id);
        if (sponsor == null)
            _logger.LogWarning("Sponsor with ID {SponsorId} not found", id);
        return sponsor;
    }

    public async Task<Sponsor> CreateAsync(Sponsor sponsor)
    {
        if (await _sponsorRepository.ExistsByNameAsync(sponsor.Name))
        {
            throw new InvalidOperationException($"Ya existe un patrocinador con el nombre '{sponsor.Name}'");
        }

        if (!IsValidEmail(sponsor.ContactEmail))
        {
            throw new InvalidOperationException("El formato del correo electrónico no es válido");
        }

        sponsor.CreatedAt = DateTime.UtcNow;
        _logger.LogInformation("Creating sponsor: {SponsorName}", sponsor.Name);
        return await _sponsorRepository.CreateAsync(sponsor);
    }

    public async Task UpdateAsync(int id, Sponsor sponsor)
    {
        var existing = await _sponsorRepository.GetByIdAsync(id);
        if (existing == null)
            throw new KeyNotFoundException($"No se encontró el patrocinador con ID {id}");

        if (await _sponsorRepository.ExistsByNameAsync(sponsor.Name, id))
        {
            throw new InvalidOperationException($"Ya existe un patrocinador con el nombre '{sponsor.Name}'");
        }

        if (!IsValidEmail(sponsor.ContactEmail))
        {
            throw new InvalidOperationException("El formato del correo electrónico no es válido");
        }

        existing.Name = sponsor.Name;
        existing.ContactEmail = sponsor.ContactEmail;
        existing.Phone = sponsor.Phone;
        existing.WebsiteUrl = sponsor.WebsiteUrl;
        existing.Category = sponsor.Category;
        existing.UpdatedAt = DateTime.UtcNow;

        _logger.LogInformation("Updating sponsor with ID: {SponsorId}", id);
        await _sponsorRepository.UpdateAsync(existing);
    }

    public async Task DeleteAsync(int id)
    {
        var exists = await _sponsorRepository.ExistsAsync(id);
        if (!exists)
            throw new KeyNotFoundException($"No se encontró el patrocinador con ID {id}");

        _logger.LogInformation("Deleting sponsor with ID: {SponsorId}", id);
        await _sponsorRepository.DeleteAsync(id);
    }

    public async Task<TournamentSponsor> LinkToTournamentAsync(int sponsorId, int tournamentId, decimal contractAmount)
    {
        var sponsor = await _sponsorRepository.GetByIdAsync(sponsorId);
        if (sponsor == null)
            throw new KeyNotFoundException($"No se encontró el patrocinador con ID {sponsorId}");

        var tournament = await _tournamentRepository.GetByIdAsync(tournamentId);
        if (tournament == null)
            throw new KeyNotFoundException($"No se encontró el torneo con ID {tournamentId}");

        if (contractAmount <= 0)
            throw new InvalidOperationException("El monto del contrato debe ser mayor a 0");

        if (await _tournamentSponsorRepository.ExistsAsync(tournamentId, sponsorId))
            throw new InvalidOperationException($"El patrocinador '{sponsor.Name}' ya está vinculado al torneo '{tournament.Name}'");

        var tournamentSponsor = new TournamentSponsor
        {
            TournamentId = tournamentId,
            SponsorId = sponsorId,
            ContractAmount = contractAmount,
            JoinedAt = DateTime.UtcNow
        };

        _logger.LogInformation("Linking sponsor {SponsorId} to tournament {TournamentId}", sponsorId, tournamentId);
        return await _tournamentSponsorRepository.CreateAsync(tournamentSponsor);
    }

    public async Task<IEnumerable<TournamentSponsor>> GetSponsorTournamentsAsync(int sponsorId)
    {
        var exists = await _sponsorRepository.ExistsAsync(sponsorId);
        if (!exists)
            throw new KeyNotFoundException($"No se encontró el patrocinador con ID {sponsorId}");

        return await _tournamentSponsorRepository.GetBySponsorIdAsync(sponsorId);
    }

    public async Task UnlinkFromTournamentAsync(int sponsorId, int tournamentId)
    {
        var link = await _tournamentSponsorRepository.GetByTournamentAndSponsorAsync(tournamentId, sponsorId);
        if (link == null)
            throw new KeyNotFoundException($"No existe vinculación entre el patrocinador {sponsorId} y el torneo {tournamentId}");

        _logger.LogInformation("Unlinking sponsor {SponsorId} from tournament {TournamentId}", sponsorId, tournamentId);
        await _tournamentSponsorRepository.DeleteAsync(link.Id);
    }

    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            return regex.IsMatch(email);
        }
        catch
        {
            return false;
        }
    }
}
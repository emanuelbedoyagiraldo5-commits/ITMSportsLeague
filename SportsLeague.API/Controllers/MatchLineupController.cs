using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SportsLeague.API.DTOs.Request;
using SportsLeague.API.DTOs.Response;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Interfaces.Services;

namespace SportsLeague.API.Controllers;

[ApiController]
[Route("api/match/{matchId}/lineup")]
public class MatchLineupController : ControllerBase
{
    private readonly IMatchLineupService _matchLineupService;
    private readonly IMapper _mapper;

    public MatchLineupController(IMatchLineupService matchLineupService, IMapper mapper)
    {
        _matchLineupService = matchLineupService;
        _mapper = mapper;
    }

    // POST /api/match/{matchId}/lineup
    [HttpPost]
    public async Task<ActionResult<MatchLineupDto>> AddToLineup(int matchId, CreateMatchLineupDto dto)
    {
        try
        {
            var lineup = _mapper.Map<MatchLineup>(dto);
            var created = await _matchLineupService.AddToLineupAsync(matchId, lineup);

            // Obtener el registro completo con las navigation properties cargadas
            var allLineups = await _matchLineupService.GetLineupByMatchAsync(matchId);
            var createdLineup = allLineups.FirstOrDefault(l => l.Id == created.Id);

            return Ok(_mapper.Map<MatchLineupDto>(createdLineup));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // GET /api/match/{matchId}/lineup
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MatchLineupDto>>> GetLineup(int matchId)
    {
        try
        {
            var lineups = await _matchLineupService.GetLineupByMatchAsync(matchId);
            return Ok(_mapper.Map<IEnumerable<MatchLineupDto>>(lineups));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // GET /api/match/{matchId}/lineup/team/{teamId}
    [HttpGet("team/{teamId}")]
    public async Task<ActionResult<IEnumerable<MatchLineupDto>>> GetLineupByTeam(int matchId, int teamId)
    {
        try
        {
            var lineups = await _matchLineupService.GetLineupByMatchAndTeamAsync(matchId, teamId);
            return Ok(_mapper.Map<IEnumerable<MatchLineupDto>>(lineups));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // DELETE /api/match/{matchId}/lineup/{id}
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteFromLineup(int matchId, int id)
    {
        try
        {
            await _matchLineupService.DeleteFromLineupAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}

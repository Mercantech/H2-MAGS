using API.Data;
using DomainModels;
using DomainModels.Mapping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    /// <summary>
    /// Controller til håndtering af rum-relaterede operationer.
    /// Indeholder funktionalitet til CRUD operationer for rum i hoteller.
    /// Implementerer struktureret fejlhåndtering med logging og try-catch.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class RoomsController : ControllerBase
    {
        private readonly AppDBContext _context;
        private readonly ILogger<RoomsController> _logger;

        /// <summary>
        /// Initialiserer en ny instans af RoomsController.
        /// </summary>
        /// <param name="context">Database context til adgang til rumdata.</param>
        /// <param name="logger">Logger til fejlrapportering.</param>
        public RoomsController(AppDBContext context, ILogger<RoomsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Henter alle rum fra systemet.
        /// </summary>
        /// <returns>En liste af alle rum med hotel information.</returns>
        /// <response code="200">Rummene blev hentet succesfuldt.</response>
        /// <response code="500">Der opstod en intern serverfejl.</response>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoomGetDto>>> GetRooms()
        {
            try
            {
                _logger.LogInformation("Henter alle rum");

                var rooms = await _context.Rooms
                    .Include(r => r.Hotel)
                    .ToListAsync();

                _logger.LogInformation("Hentet {RoomCount} rum succesfuldt", rooms.Count);
                return Ok(RoomMapping.ToRoomGetDtos(rooms));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl ved hentning af alle rum");
                return StatusCode(500, "Der opstod en intern serverfejl ved hentning af rum");
            }
        }

        /// <summary>
        /// Henter et specifikt rum baseret på ID.
        /// </summary>
        /// <param name="id">Unikt ID for rummet.</param>
        /// <returns>Rummets information.</returns>
        /// <response code="200">Rummet blev fundet og returneret.</response>
        /// <response code="404">Rum med det angivne ID blev ikke fundet.</response>
        /// <response code="500">Der opstod en intern serverfejl.</response>
        [HttpGet("{id}")]
        public async Task<ActionResult<RoomGetDto>> GetRoom(string id)
        {
            try
            {
                _logger.LogInformation("Henter rum med ID: {RoomId}", id);

                var room = await _context.Rooms
                    .Include(r => r.Hotel)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (room == null)
                {
                    _logger.LogWarning("Rum med ID {RoomId} ikke fundet", id);
                    return NotFound();
                }

                _logger.LogInformation("Rum med ID {RoomId} hentet succesfuldt", id);
                return Ok(RoomMapping.ToRoomGetDto(room));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl ved hentning af rum med ID: {RoomId}", id);
                return StatusCode(500, "Der opstod en intern serverfejl ved hentning af rum");
            }
        }

        /// <summary>
        /// Henter alle rum for et specifikt hotel.
        /// </summary>
        /// <param name="hotelId">Hotel ID for at filtrere rum.</param>
        /// <returns>Liste af rum for det angivne hotel.</returns>
        /// <response code="200">Rummene blev hentet succesfuldt.</response>
        /// <response code="404">Hotel med det angivne ID blev ikke fundet.</response>
        /// <response code="500">Der opstod en intern serverfejl.</response>
        [HttpGet("hotel/{hotelId}")]
        public async Task<ActionResult<IEnumerable<RoomGetDto>>> GetRoomsByHotel(string hotelId)
        {
            try
            {
                _logger.LogInformation("Henter rum for hotel: {HotelId}", hotelId);

                // Tjek om hotellet eksisterer
                var hotelExists = await _context.Hotels.AnyAsync(h => h.Id == hotelId);
                if (!hotelExists)
                {
                    _logger.LogWarning("Hotel med ID {HotelId} ikke fundet", hotelId);
                    return NotFound("Hotel ikke fundet");
                }

                var rooms = await _context.Rooms
                    .Include(r => r.Hotel)
                    .Where(r => r.HotelId == hotelId)
                    .ToListAsync();

                _logger.LogInformation("Hentet {RoomCount} rum for hotel {HotelId}", rooms.Count, hotelId);
                return Ok(RoomMapping.ToRoomGetDtos(rooms));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl ved hentning af rum for hotel: {HotelId}", hotelId);
                return StatusCode(500, "Der opstod en intern serverfejl ved hentning af rum");
            }
        }

        /// <summary>
        /// Opdaterer et eksisterende rum.
        /// </summary>
        /// <param name="id">ID på rummet der skal opdateres.</param>
        /// <param name="roomPutDto">Opdaterede rumdata.</param>
        /// <returns>Bekræftelse på opdateringen.</returns>
        /// <response code="204">Rummet blev opdateret succesfuldt.</response>
        /// <response code="400">Ugyldig forespørgsel - ID matcher ikke rum ID.</response>
        /// <response code="404">Rum med det angivne ID blev ikke fundet.</response>
        /// <response code="500">Der opstod en intern serverfejl.</response>
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRoom(string id, RoomPutDto roomPutDto)
        {
            if (id != roomPutDto.Id)
            {
                return BadRequest("ID i URL matcher ikke rum ID");
            }

            try
            {
                _logger.LogInformation("Opdaterer rum med ID: {RoomId}", id);

                var room = await _context.Rooms.FindAsync(id);
                if (room == null)
                {
                    _logger.LogWarning("Rum med ID {RoomId} ikke fundet for opdatering", id);
                    return NotFound();
                }

                // Tjek om det nye hotel eksisterer
                var hotelExists = await _context.Hotels.AnyAsync(h => h.Id == roomPutDto.HotelId);
                if (!hotelExists)
                {
                    _logger.LogWarning("Hotel med ID {HotelId} ikke fundet ved opdatering af rum {RoomId}", roomPutDto.HotelId, id);
                    return BadRequest("Det angivne hotel eksisterer ikke");
                }

                RoomMapping.UpdateRoomFromPutDto(room, roomPutDto);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Rum med ID {RoomId} opdateret succesfuldt", id);
                return NoContent();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex, "Concurrency konflikt ved opdatering af rum: {RoomId}", id);
                
                if (!await RoomExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl ved opdatering af rum: {RoomId}", id);
                return StatusCode(500, "Der opstod en intern serverfejl ved opdatering af rum");
            }
        }

        /// <summary>
        /// Opretter et nyt rum i systemet.
        /// </summary>
        /// <param name="roomPostDto">Data for det nye rum.</param>
        /// <returns>Det oprettede rum.</returns>
        /// <response code="201">Rummet blev oprettet succesfuldt.</response>
        /// <response code="400">Ugyldig forespørgsel eller rumdata.</response>
        /// <response code="409">Et rum med samme nummer eksisterer allerede i hotellet.</response>
        /// <response code="500">Der opstod en intern serverfejl.</response>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<RoomGetDto>> PostRoom(RoomPostDto roomPostDto)
        {
            try
            {
                _logger.LogInformation("Opretter nyt rum {RoomNumber} for hotel {HotelId}", roomPostDto.Number, roomPostDto.HotelId);

                // Tjek om hotellet eksisterer
                var hotelExists = await _context.Hotels.AnyAsync(h => h.Id == roomPostDto.HotelId);
                if (!hotelExists)
                {
                    _logger.LogWarning("Forsøg på at oprette rum for ikke-eksisterende hotel: {HotelId}", roomPostDto.HotelId);
                    return BadRequest("Det angivne hotel eksisterer ikke");
                }

                // Tjek om rum nummer allerede eksisterer i hotellet
                var roomExists = await _context.Rooms.AnyAsync(r => r.HotelId == roomPostDto.HotelId && r.Number == roomPostDto.Number);
                if (roomExists)
                {
                    _logger.LogWarning("Rum nummer {RoomNumber} eksisterer allerede i hotel {HotelId}", roomPostDto.Number, roomPostDto.HotelId);
                    return Conflict("Et rum med dette nummer eksisterer allerede i hotellet");
                }

                var room = RoomMapping.ToRoomFromPostDto(roomPostDto);
                _context.Rooms.Add(room);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Nyt rum oprettet succesfuldt med ID: {RoomId}", room.Id);

                // Hent det oprettede rum med hotel information
                var createdRoom = await _context.Rooms
                    .Include(r => r.Hotel)
                    .FirstOrDefaultAsync(r => r.Id == room.Id);

                return CreatedAtAction("GetRoom", new { id = room.Id }, RoomMapping.ToRoomGetDto(createdRoom!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl ved oprettelse af rum: {RoomNumber} for hotel {HotelId}", roomPostDto?.Number, roomPostDto?.HotelId);
                return StatusCode(500, "Der opstod en intern serverfejl ved oprettelse af rum");
            }
        }

        /// <summary>
        /// Sletter et rum fra systemet.
        /// </summary>
        /// <param name="id">ID på rummet der skal slettes.</param>
        /// <returns>Bekræftelse på sletningen.</returns>
        /// <response code="204">Rummet blev slettet succesfuldt.</response>
        /// <response code="400">Rummet kan ikke slettes da det har aktive bookinger.</response>
        /// <response code="404">Rum med det angivne ID blev ikke fundet.</response>
        /// <response code="500">Der opstod en intern serverfejl.</response>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoom(string id)
        {
            try
            {
                _logger.LogInformation("Sletter rum med ID: {RoomId}", id);

                var room = await _context.Rooms
                    .Include(r => r.Bookings)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (room == null)
                {
                    _logger.LogWarning("Rum med ID {RoomId} ikke fundet for sletning", id);
                    return NotFound();
                }

                // Tjek om rummet har aktive bookinger
                var hasActiveBookings = room.Bookings.Any(b => b.EndDate > DateTime.UtcNow);
                if (hasActiveBookings)
                {
                    _logger.LogWarning("Forsøg på at slette rum {RoomId} med aktive bookinger", id);
                    return BadRequest("Rummet kan ikke slettes da det har aktive bookinger");
                }

                _context.Rooms.Remove(room);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Rum med ID {RoomId} slettet succesfuldt", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl ved sletning af rum: {RoomId}", id);
                return StatusCode(500, "Der opstod en intern serverfejl ved sletning af rum");
            }
        }

        /// <summary>
        /// Hjælpemetode til at kontrollere om et rum eksisterer.
        /// </summary>
        /// <param name="id">ID på rummet der skal kontrolleres.</param>
        /// <returns>True hvis rummet eksisterer, ellers false.</returns>
        private async Task<bool> RoomExists(string id)
        {
            return await _context.Rooms.AnyAsync(e => e.Id == id);
        }
    }
}

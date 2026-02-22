using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Data;
using DomainModels;
using DomainModels.Mapping;

namespace API.Controllers
{
    /// <summary>
    /// Controller til håndtering af hotel-relaterede operationer.
    /// Giver adgang til CRUD-operationer for hoteller i systemet.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class HotelsController : ControllerBase
    {
        private readonly AppDBContext _context;

        /// <summary>
        /// Initialiserer en ny instans af HotelsController.
        /// </summary>
        /// <param name="context">Database context til adgang til hoteldata.</param>
        public HotelsController(AppDBContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Henter alle hoteller fra systemet.
        /// </summary>
        /// <returns>En liste af alle hoteller i systemet.</returns>
        /// <response code="200">Hotellerne blev hentet succesfuldt.</response>
        /// <response code="500">Der opstod en intern serverfejl.</response>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<HotelGetDto>>> GetHotels()
        {
            var hotels = await _context.Hotels.ToListAsync();
            return HotelMapping.ToHotelGetDtos(hotels);


        }

        /// <summary>
        /// Henter et specifikt hotel baseret på ID.
        /// </summary>
        /// <param name="id">Unikt ID for hotellet.</param>
        /// <returns>Hotellet med det angivne ID.</returns>
        /// <response code="200">Hotellet blev fundet og returneret.</response>
        /// <response code="404">Hotel med det angivne ID blev ikke fundet.</response>
        /// <response code="500">Der opstod en intern serverfejl.</response>
        [HttpGet("{id}")]
        public async Task<ActionResult<HotelGetDto>> GetHotel(string id)
        {
            var hotel = await _context.Hotels.FindAsync(id);

            if (hotel == null)
            {
                return NotFound();
            }

            return HotelMapping.ToHotelGetDto(hotel);
        }

        /// <summary>
        /// Opdaterer et eksisterende hotel.
        /// </summary>
        /// <param name="id">ID på hotellet der skal opdateres.</param>
        /// <param name="hotelDto">Opdaterede hoteldata.</param>
        /// <returns>Bekræftelse på opdateringen.</returns>
        /// <response code="204">Hotellet blev opdateret succesfuldt.</response>
        /// <response code="400">Ugyldig forespørgsel - ID matcher ikke hotel ID.</response>
        /// <response code="404">Hotel med det angivne ID blev ikke fundet.</response>
        /// <response code="500">Der opstod en intern serverfejl.</response>
        /// <remarks>
        /// For at beskytte mod overposting angreb, se https://go.microsoft.com/fwlink/?linkid=2123754
        /// </remarks>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutHotel(string id, HotelPutDto hotelDto)
        {
            if (id != hotelDto.Id)
            {
                return BadRequest();
            }

            var hotel = await _context.Hotels.FindAsync(id);
            if (hotel == null)
            {
                return NotFound();
            }

            HotelMapping.UpdateHotelFromDto(hotel, hotelDto);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!HotelExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        /// <summary>
        /// Opretter et nyt hotel i systemet.
        /// </summary>
        /// <param name="hotelDto">Data for det nye hotel.</param>
        /// <returns>Det oprettede hotel.</returns>
        /// <response code="201">Hotellet blev oprettet succesfuldt.</response>
        /// <response code="400">Ugyldig forespørgsel eller hoteldata.</response>
        /// <response code="409">Et hotel med samme ID eksisterer allerede.</response>
        /// <response code="500">Der opstod en intern serverfejl.</response>
        /// <remarks>
        /// For at beskytte mod overposting angreb, se https://go.microsoft.com/fwlink/?linkid=2123754
        /// </remarks>
        [HttpPost]
        public async Task<ActionResult<Hotel>> PostHotel(HotelPostDto hotelDto)
        {
            Hotel hotel = HotelMapping.ToHotelFromDto(hotelDto);
            _context.Hotels.Add(hotel);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (HotelExists(hotel.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetHotel", new { id = hotel.Id }, hotel);
        }

        /// <summary>
        /// Sletter et hotel fra systemet.
        /// </summary>
        /// <param name="id">ID på hotellet der skal slettes.</param>
        /// <returns>Bekræftelse på sletningen.</returns>
        /// <response code="204">Hotellet blev slettet succesfuldt.</response>
        /// <response code="404">Hotel med det angivne ID blev ikke fundet.</response>
        /// <response code="500">Der opstod en intern serverfejl.</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHotel(string id)
        {
            var hotel = await _context.Hotels.FindAsync(id);
            if (hotel == null)
            {
                return NotFound();
            }

            _context.Hotels.Remove(hotel);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Hjælpemetode til at kontrollere om et hotel eksisterer.
        /// </summary>
        /// <param name="id">ID på hotellet der skal kontrolleres.</param>
        /// <returns>True hvis hotellet eksisterer, ellers false.</returns>
        private bool HotelExists(string id)
        {
            return _context.Hotels.Any(e => e.Id == id);
        }

    }
}

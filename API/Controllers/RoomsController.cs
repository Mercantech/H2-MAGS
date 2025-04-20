using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.DBContext;
using DomainModels;
using DomainModels.DTOs;
using DomainModels.DTOs.Room;
using DomainModels.DTOs.RoomType;

namespace API.Controllers
{
    /// <summary>
    /// API-controller til håndtering af værelser.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class RoomsController : ControllerBase
    {
        private readonly HotelContext _context;

        public RoomsController(HotelContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Henter alle værelser.
        /// </summary>
        /// <returns>En liste af værelser.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetRoomDTO>>> GetRooms()
        {
            return await _context.Rooms
                .Select(r => new GetRoomDTO
                {
                    Id = r.Id,
                    RoomNumber = r.RoomNumber,
                    RoomTypeId = r.RoomTypeId,
                    PricePerNight = r.PricePerNight
                })
                .ToListAsync();
        }

        /// <summary>
        /// Henter et specifikt værelse ud fra værelsets ID.
        /// </summary>
        /// <param name="id">Værelsets unikke ID.</param>
        /// <returns>Værelsets detaljer.</returns>
        /// <response code="404">Værelse ikke fundet.</response>
        [HttpGet("{id}")]
        public async Task<ActionResult<GetRoomDTO>> GetRoom(string id)
        {
            var room = await _context.Rooms
                .Select(r => new GetRoomDTO
                {
                    Id = r.Id,
                    RoomNumber = r.RoomNumber,
                    RoomTypeId = r.RoomTypeId,
                    PricePerNight = r.PricePerNight
                })
                .FirstOrDefaultAsync(r => r.Id == id);

            if (room == null)
            {
                return NotFound();
            }

            return room;
        }

        /// <summary>
        /// Henter detaljer for alle værelser inkl. værelsestype.
        /// </summary>
        /// <returns>En liste af værelser med detaljer.</returns>
        [HttpGet("details")]
        public async Task<ActionResult<IEnumerable<RoomDetailsDTO>>> GetRoomDetails()
        {
            return await _context.Rooms
                .Include(r => r.RoomType)
                .Select(r => new RoomDetailsDTO
                {
                    Id = r.Id,
                    RoomNumber = r.RoomNumber,
                    RoomTypeId = r.RoomTypeId,
                    PricePerNight = r.PricePerNight,
                    RoomType = new GetRoomTypeDTO
                    {
                        Id = r.RoomType.Id,
                        Name = r.RoomType.Name,
                        Size = r.RoomType.Size,
                        Description = r.RoomType.Description
                    }
                })
                .ToListAsync();
        }

        /// <summary>
        /// Opdaterer et eksisterende værelse.
        /// </summary>
        /// <param name="id">Værelsets unikke ID.</param>
        /// <param name="getRoomDto">Objekt med opdaterede værelsesdata.</param>
        /// <returns>NoContent ved succes, ellers fejlbesked.</returns>
        /// <response code="400">ID matcher ikke.</response>
        /// <response code="404">Værelse ikke fundet.</response>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRoom(string id, GetRoomDTO getRoomDto)
        {
            if (id != getRoomDto.Id)
            {
                return BadRequest();
            }

            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
            {
                return NotFound();
            }

            room.RoomNumber = getRoomDto.RoomNumber;
            room.RoomTypeId = getRoomDto.RoomTypeId;
            room.PricePerNight = getRoomDto.PricePerNight;
            room.UpdatedAt = DateTime.UtcNow;

            _context.Entry(room).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RoomExists(id))
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
        /// Opretter et nyt værelse.
        /// </summary>
        /// <param name="createRoomDto">Objekt med værelsesdata.</param>
        /// <returns>Det oprettede værelse.</returns>
        [HttpPost]
        public async Task<ActionResult<CreateRoomDTO>> PostRoom(CreateRoomDTO createRoomDto)
        {
            var room = new Room
            {
                Id = Guid.NewGuid().ToString("N"),
                RoomNumber = createRoomDto.RoomNumber,
                RoomTypeId = createRoomDto.RoomTypeId,
                PricePerNight = createRoomDto.PricePerNight,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRoom), new { id = room.Id }, createRoomDto);
        }

        /// <summary>
        /// Sletter et værelse ud fra værelsets ID.
        /// </summary>
        /// <param name="id">Værelsets unikke ID.</param>
        /// <returns>NoContent ved succes, ellers fejlbesked.</returns>
        /// <response code="404">Værelse ikke fundet.</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoom(string id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
            {
                return NotFound();
            }

            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool RoomExists(string id)
        {
            return _context.Rooms.Any(e => e.Id == id);
        }
    }
}

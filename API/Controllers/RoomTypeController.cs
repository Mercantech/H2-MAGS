using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.DBContext;
using DomainModels;
using DomainModels.DTOs;
using DomainModels.DTOs.RoomType;
using Microsoft.AspNetCore.Authorization;
namespace API.Controllers
{
    /// <summary>
    /// API-controller til håndtering af værelsestyper.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class RoomTypesController : ControllerBase
    {
        private readonly HotelContext _context;

        public RoomTypesController(HotelContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Henter alle værelsestyper.
        /// </summary>
        /// <returns>En liste af værelsestyper.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetRoomTypeDTO>>> GetRoomTypes()
        {
            return await _context.RoomTypes
                .Select(rt => new GetRoomTypeDTO
                {
                    Id = rt.Id,
                    Name = rt.Name,
                    Size = rt.Size,
                    Description = rt.Description
                })
                .ToListAsync();
        }

        /// <summary>
        /// Henter en specifik værelsestype ud fra ID.
        /// </summary>
        /// <param name="id">Værelsestypens unikke ID.</param>
        /// <returns>Værelsestypens detaljer.</returns>
        /// <response code="404">Værelsestype ikke fundet.</response>
        [HttpGet("{id}")]
        public async Task<ActionResult<GetRoomTypeDTO>> GetRoomType(string id)
        {
            var roomType = await _context.RoomTypes
                .Select(rt => new GetRoomTypeDTO
                {
                    Id = rt.Id,
                    Name = rt.Name,
                    Size = rt.Size,
                    Description = rt.Description
                })
                .FirstOrDefaultAsync(rt => rt.Id == id);

            if (roomType == null)
            {
                return NotFound();
            }

            return roomType;
        }

        /// <summary>
        /// Opdaterer en eksisterende værelsestype.
        /// </summary>
        /// <param name="id">Værelsestypens unikke ID.</param>
        /// <param name="createRoomTypeDto">Objekt med opdaterede værelsestypedata.</param>
        /// <returns>NoContent ved succes, ellers fejlbesked.</returns>
        /// <response code="404">Værelsestype ikke fundet.</response>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRoomType(string id, CreateRoomTypeDTO createRoomTypeDto)
        {


            var roomType = await _context.RoomTypes.FindAsync(id);
            if (roomType == null)
            {
                return NotFound();
            }

            roomType.Name = createRoomTypeDto.Name;
            roomType.Size = createRoomTypeDto.Size;
            roomType.Description = createRoomTypeDto.Description;

            _context.Entry(roomType).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RoomTypeExists(id))
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
        /// Opretter en ny værelsestype.
        /// </summary>
        /// <param name="createRoomTypeDto">Objekt med værelsestypedata.</param>
        /// <returns>Den oprettede værelsestype.</returns>
        [HttpPost]
        public async Task<ActionResult<CreateRoomTypeDTO>> PostRoomType(CreateRoomTypeDTO createRoomTypeDto)
        {
            var roomType = new RoomType
            {
                Id = Guid.NewGuid().ToString("N"),
                Name = createRoomTypeDto.Name,
                Size = createRoomTypeDto.Size,
                Description = createRoomTypeDto.Description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.RoomTypes.Add(roomType);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRoomType), new { id = roomType.Id }, createRoomTypeDto);
        }

        /// <summary>
        /// Sletter en værelsestype ud fra ID.
        /// </summary>
        /// <param name="id">Værelsestypens unikke ID.</param>
        /// <returns>NoContent ved succes, ellers fejlbesked.</returns>
        /// <response code="404">Værelsestype ikke fundet.</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoomType(string id)
        {
            var roomType = await _context.RoomTypes.FindAsync(id);
            if (roomType == null)
            {
                return NotFound();
            }

            _context.RoomTypes.Remove(roomType);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool RoomTypeExists(string id)
        {
            return _context.RoomTypes.Any(e => e.Id == id);
        }
    }
}

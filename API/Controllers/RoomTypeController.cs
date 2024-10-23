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

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomTypesController : ControllerBase
    {
        private readonly HotelContext _context;

        public RoomTypesController(HotelContext context)
        {
            _context = context;
        }

        // GET: api/RoomTypes
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

        // GET: api/RoomTypes/5
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

        // PUT: api/RoomTypes/5
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

        // POST: api/RoomTypes
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

        // DELETE: api/RoomTypes/5
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

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PricingMvp.Application.DTOs;
using PricingMvp.Application.Interfaces;
using PricingMvp.Domain.Entities;

namespace PricingMvp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Requiere autenticación
    public class RoomsController : ControllerBase
    {
        private readonly IApplicationDbContext _context;
        
        public RoomsController(IApplicationDbContext context)
        {
            _context = context;
        }
        
        // GET: api/rooms
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoomDto>>> GetRooms()
        {
            var rooms = await _context.Rooms
                .Include(r => r.Hotel)
                .Where(r => !r.IsDeleted)
                .Select(r => new RoomDto
                {
                    Id = r.Id,
                    RoomNumber = r.RoomNumber,
                    Type = r.Type.ToString(),
                    BasePrice = r.BasePrice,
                    Capacity = r.Capacity,
                    HotelName = r.Hotel.Name
                })
                .ToListAsync();
            
            return Ok(rooms);
        }
        
        // GET: api/rooms/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RoomDto>> GetRoom(int id)
        {
            var room = await _context.Rooms
                .Include(r => r.Hotel)
                .Where(r => r.Id == id && !r.IsDeleted)
                .Select(r => new RoomDto
                {
                    Id = r.Id,
                    RoomNumber = r.RoomNumber,
                    Type = r.Type.ToString(),
                    BasePrice = r.BasePrice,
                    Capacity = r.Capacity,
                    HotelName = r.Hotel.Name
                })
                .FirstOrDefaultAsync();
            
            if (room == null)
                return NotFound(new { message = $"Habitación {id} no encontrada" });
            
            return Ok(room);
        }
        
        // POST: api/rooms
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<RoomDto>> CreateRoom([FromBody] CreateRoomDto dto)
        {
            var room = new Room
            {
                HotelId = dto.HotelId,
                RoomNumber = dto.RoomNumber,
                Type = Enum.Parse<Domain.Enums.RoomType>(dto.Type),
                BasePrice = dto.BasePrice,
                Capacity = dto.Capacity,
                Quantity = dto.Quantity,
                HasBalcony = dto.HasBalcony,
                HasSeaView = dto.HasSeaView,
                SquareMeters = dto.SquareMeters
            };
            
            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();
            
            return CreatedAtAction(nameof(GetRoom), new { id = room.Id }, 
                new RoomDto
                {
                    Id = room.Id,
                    RoomNumber = room.RoomNumber,
                    Type = room.Type.ToString(),
                    BasePrice = room.BasePrice,
                    Capacity = room.Capacity
                });
        }
        
        // PUT: api/rooms/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,RevenueManager")]
        public async Task<IActionResult> UpdateRoom(int id, [FromBody] UpdateRoomDto dto)
        {
            var room = await _context.Rooms.FindAsync(id);
            
            if (room == null)
                return NotFound();
            
            room.BasePrice = dto.BasePrice;
            room.Capacity = dto.Capacity;
            room.HasBalcony = dto.HasBalcony;
            room.HasSeaView = dto.HasSeaView;
            
            await _context.SaveChangesAsync();
            
            return NoContent();
        }
        
        // DELETE: api/rooms/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            
            if (room == null)
                return NotFound();
            
            room.IsDeleted = true; // Soft delete
            await _context.SaveChangesAsync();
            
            return NoContent();
        }
    }
}
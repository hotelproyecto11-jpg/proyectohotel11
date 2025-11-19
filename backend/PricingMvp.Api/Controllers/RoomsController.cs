using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PricingMvp.Application.DTOs;
using PricingMvp.Application.Interfaces;
using PricingMvp.Domain.Entities;
using System.Security.Claims;

namespace PricingMvp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Requiere autenticación JWT
    public class RoomsController : ControllerBase
    {
        private readonly IApplicationDbContext _context;
        
        public RoomsController(IApplicationDbContext context)
        {
            _context = context;
        }
        
        // GET: /api/rooms - Obtiene todas las habitaciones (filtra por hotel si es admin regular)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoomDto>>> GetRooms([FromQuery] int? hotelId = null)
        {
            // Obtener email y rol del usuario autenticado desde el JWT token
            var currentUserEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var currentUserRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            
            var query = _context.Rooms
                .Include(r => r.Hotel)
                .Where(r => !r.IsDeleted);
            
            // Si es Admin y no es admin@pricingmvp.com, filtrar por su hotel
            if (currentUserRole == "Admin" && currentUserEmail != "admin@pricingmvp.com")
            {
                var currentAdmin = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == currentUserEmail);
                
                if (currentAdmin?.HotelId == null)
                    return BadRequest(new { message = "El admin no está asignado a un hotel" });
                
                query = query.Where(r => r.HotelId == currentAdmin.HotelId);
            }
            // Si se especifica hotelId en la query, aplicar ese filtro
            else if (hotelId.HasValue)
            {
                query = query.Where(r => r.HotelId == hotelId.Value);
            }
            
            var rooms = await query
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
        
        // GET: /api/rooms/{id} - Obtiene una habitación específica por ID
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
        
        // POST: /api/rooms - Crea una nueva habitación (solo Admin)
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
        
        // PUT: /api/rooms/{id} - Actualiza una habitación existente (Admin y RevenueManager)
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
        
        // DELETE: /api/rooms/{id} - Elimina una habitación (solo Admin)
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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PricingMvp.Application.Interfaces;
using PricingMvp.Application.DTOs;

namespace PricingMvp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class HotelsController : ControllerBase
    {
        private readonly IApplicationDbContext _context;

        public HotelsController(IApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/hotels
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<object>>> GetHotels()
        {
            var hotels = await _context.Hotels
                .Select(h => new { id = h.Id, name = h.Name })
                .ToListAsync();

            return Ok(hotels);
        }

        // POST: api/hotels
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<HotelDto>> CreateHotel([FromBody] PricingMvp.Application.DTOs.CreateHotelDto dto)
        {
            var hotel = new PricingMvp.Domain.Entities.Hotel
            {
                Name = dto.Name,
                City = dto.City,
                State = dto.State,
                Address = dto.Address,
                Stars = dto.Stars,
                Description = dto.Description
            };

            _context.Hotels.Add(hotel);
            await _context.SaveChangesAsync();

            var result = new PricingMvp.Application.DTOs.HotelDto
            {
                Id = hotel.Id,
                Name = hotel.Name,
                City = hotel.City,
                Stars = hotel.Stars
            };

            return CreatedAtAction(nameof(GetHotels), new { id = hotel.Id }, result);
        }

        // GET: api/hotels/5
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<HotelDto>> GetHotel(int id)
        {
            var hotel = await _context.Hotels.FindAsync(id);
            if (hotel == null)
                return NotFound(new { message = "Hotel no encontrado" });

            var dto = new HotelDto { Id = hotel.Id, Name = hotel.Name, City = hotel.City, Stars = hotel.Stars };
            return Ok(dto);
        }

        // PUT: api/hotels/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateHotel(int id, [FromBody] CreateHotelDto dto)
        {
            var hotel = await _context.Hotels.FindAsync(id);
            if (hotel == null)
                return NotFound(new { message = "Hotel no encontrado" });

            hotel.Name = dto.Name;
            hotel.City = dto.City;
            hotel.State = dto.State;
            hotel.Address = dto.Address;
            hotel.Stars = dto.Stars;
            hotel.Description = dto.Description;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/hotels/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteHotel(int id)
        {
            var hotel = await _context.Hotels.FindAsync(id);
            if (hotel == null)
                return NotFound(new { message = "Hotel no encontrado" });

            // Soft delete pattern isn't implemented for Hotel; performing hard delete
            _context.Hotels.Remove(hotel);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

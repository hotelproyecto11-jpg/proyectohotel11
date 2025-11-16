using Microsoft.AspNetCore.Mvc;
using PricingMvp.Application.Interfaces;
using PricingMvp.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace PricingMvp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MlController : ControllerBase
    {
        private readonly IApplicationDbContext _context;
        private readonly MlPricingClient _mlClient;

        public MlController(IApplicationDbContext context, MlPricingClient mlClient)
        {
            _context = context;
            _mlClient = mlClient;
        }

        // POST: api/ml/train
        // Exporta datos de PriceHistory (últimos 180 días) y llama al servicio ML /train
        [HttpPost("train")]
        public async Task<IActionResult> Train()
        {
            var cutoff = DateTime.UtcNow.AddDays(-180);
            var rows = await _context.PriceHistories
                .Include(p => p.Room)
                .Where(p => p.Date >= cutoff && p.Price > 0)
                .Select(p => new
                {
                    roomId = p.RoomId,
                    basePrice = p.Room.BasePrice,
                    hotelOccupancy = p.OccupancyPercent,
                    dayOfWeek = ((int)p.Date.DayOfWeek),
                    month = p.Date.Month,
                    isWeekend = (p.Date.DayOfWeek == DayOfWeek.Friday || p.Date.DayOfWeek == DayOfWeek.Saturday),
                    capacity = p.Room.Capacity,
                    hasSeaView = p.Room.HasSeaView,
                    price = p.Price
                })
                .ToListAsync();

            var ok = await _mlClient.TrainAsync(rows);
            if (!ok) return StatusCode(500, new { message = "No se pudo entrenar el modelo (ML service)." });
            return Ok(new { message = "Entrenamiento solicitado" });
        }
    }
}

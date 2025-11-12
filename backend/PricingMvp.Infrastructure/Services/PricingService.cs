using Microsoft.EntityFrameworkCore;
using PricingMvp.Application.Interfaces;
using PricingMvp.Domain.Entities;

namespace PricingMvp.Infrastructure.Services
{
    public class PricingService : IPricingService
    {
        private readonly IApplicationDbContext _context;
        
        public PricingService(IApplicationDbContext context)
        {
            _context = context;
        }
        
        public async Task<decimal> CalculateSuggestedPriceAsync(int roomId, DateTime targetDate)
        {
            // 1. Obtener la habitación
            var room = await _context.Rooms
                .Include(r => r.PriceHistories)
                .FirstOrDefaultAsync(r => r.Id == roomId);
                
            if (room == null)
                throw new Exception($"Room {roomId} not found");
            
            // 2. Obtener historial reciente (últimos 30 días)
            var recentHistory = room.PriceHistories
                .Where(p => p.Date >= DateTime.UtcNow.AddDays(-30))
                .OrderByDescending(p => p.Date)
                .ToList();
            
            // 3. Calcular ocupación promedio
            var avgOccupancy = recentHistory.Any() 
                ? recentHistory.Average(p => p.OccupancyPercent) 
                : 60;
            
            // 4. Aplicar reglas de pricing
            decimal basePrice = room.BasePrice;
            decimal suggestedPrice = basePrice;
            
            // Regla 1: Ajuste por ocupación
            if (avgOccupancy > 80)
                suggestedPrice *= 1.15m; // Subir 15%
            else if (avgOccupancy > 65)
                suggestedPrice *= 1.08m; // Subir 8%
            else if (avgOccupancy < 40)
                suggestedPrice *= 0.90m; // Bajar 10%
            
            // Regla 2: Ajuste por día de semana
            if (targetDate.DayOfWeek == DayOfWeek.Friday || 
                targetDate.DayOfWeek == DayOfWeek.Saturday)
            {
                suggestedPrice *= 1.12m; // Fin de semana +12%
            }
            
            // Regla 3: Ajuste por temporada (ejemplo simplificado)
            int month = targetDate.Month;
            if (month >= 12 || month <= 2) // Temporada alta
                suggestedPrice *= 1.20m;
            else if (month >= 6 && month <= 8) // Temporada media
                suggestedPrice *= 1.10m;
            
            // Redondear a 2 decimales
            return Math.Round(suggestedPrice, 2);
        }
        
        public async Task<bool> ApplySuggestedPriceAsync(int roomId, decimal newPrice, DateTime effectiveDate)
        {
            var priceHistory = new PriceHistory
            {
                RoomId = roomId,
                Date = effectiveDate,
                Price = newPrice,
                OccupancyPercent = 0, // Se actualizará después
                PredictedPrice = newPrice
            };
            
            _context.PriceHistories.Add(priceHistory);
            await _context.SaveChangesAsync();
            
            return true;
        }
    }
}
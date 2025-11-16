using Microsoft.EntityFrameworkCore;
using PricingMvp.Application.Interfaces;
using PricingMvp.Domain.Entities;

namespace PricingMvp.Infrastructure.Services
{
    public class PricingService : IPricingService
    {
        private readonly IApplicationDbContext _context;
        private readonly MlPricingClient? _mlClient;

        public PricingService(IApplicationDbContext context, MlPricingClient? mlClient = null)
        {
            _context = context;
            _mlClient = mlClient;
        }
        
        public async Task<PricingMvp.Application.DTOs.SuggestedPriceResultDto> CalculateSuggestedPriceAsync(int roomId, DateTime targetDate)
        {
            // 1. Obtener la habitación
            var room = await _context.Rooms
                .Include(r => r.PriceHistories)
                .FirstOrDefaultAsync(r => r.Id == roomId);
                
            if (room == null)
                throw new Exception($"Room {roomId} not found");
            
            // 2. Obtener historial reciente (últimos 30 días)
            // Excluir entradas sin ocupación registrada (OccupancyPercent == 0)
            var recentHistory = room.PriceHistories
                .Where(p => p.Date >= DateTime.UtcNow.AddDays(-30) && p.OccupancyPercent > 0)
                .OrderByDescending(p => p.Date)
                .ToList();

            // Obtener tambien el historial del hotel (últimos 30 días) para fallback
            var cutoffDate = DateTime.UtcNow.AddDays(-30);
            // Para el cálculo de ocupación, tomar sólo entradas con OccupancyPercent > 0
            var hotelHistory = await _context.PriceHistories
                .Include(p => p.Room)
                .Where(p => p.Room.HotelId == room.HotelId && p.Date >= cutoffDate && p.OccupancyPercent > 0)
                .ToListAsync();

            // 3. Calcular ocupación promedio
            var avgOccupancy = 0.0;
            var hadHistory = false;

            if (recentHistory.Any())
            {
                avgOccupancy = recentHistory.Average(p => p.OccupancyPercent);
                hadHistory = true;
            }
            else if (hotelHistory.Any())
            {
                avgOccupancy = hotelHistory.Average(p => p.OccupancyPercent);
                hadHistory = true;
            }
            else
            {
                // Fallback final
                avgOccupancy = 60;
            }

            // 4. Determinar precio base a partir de datos disponibles
            // Si la habitación no tiene BasePrice definido, intentar usar el promedio
            // de precios recientes de la habitación, luego del hotel, y si no hay nada usar un valor por defecto.
            decimal basePrice = room.BasePrice;
            if (basePrice <= 0)
            {
                if (recentHistory.Any())
                {
                    basePrice = recentHistory.Where(p => p.Price > 0).Average(p => p.Price);
                }
                else if (hotelHistory.Any())
                {
                    basePrice = hotelHistory.Where(p => p.Price > 0).Average(p => p.Price);
                }
                else
                {
                    basePrice = 500m; // Valor por defecto razonable para evitar 0
                }
            }

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

            // Si no hay historial (ni de la habitación ni del hotel) y el resultado
            // es igual (o casi igual) al precio base, aplicar una variación mínima
            // basada en la ocupación del hotel para producir subidas o bajadas.
            if (!hadHistory)
            {
                var multiplier = suggestedPrice / basePrice;
                if (Math.Abs(multiplier - 1m) < 0.03m)
                {
                    // ratio: -1 .. 1  (ocupación 20 => -1, ocupación 100 => +1)
                    var diff = avgOccupancy - 60.0; // centrar en 60
                    var ratio = Math.Max(-1.0, Math.Min(1.0, diff / 40.0));
                    // variation between 3% and 5% depending on magnitude
                    decimal variation = 0.03m + (decimal)(Math.Abs(ratio) * 0.02);

                    if (ratio > 0)
                        suggestedPrice *= 1 + variation; // subir
                    else if (ratio < 0)
                        suggestedPrice *= 1 - variation; // bajar
                    // si ratio == 0 => dejar igual
                }
            }

            // Intentar consultar servicio ML si está disponible
            string priceSource;
            string? modelVersion = null;

            if (_mlClient != null)
            {
                var mlReq = new MlPredictRequest
                {
                    roomId = room.Id,
                    basePrice = basePrice,
                    hotelOccupancy = avgOccupancy,
                    dayOfWeek = (int)targetDate.DayOfWeek,
                    month = targetDate.Month,
                    isWeekend = (targetDate.DayOfWeek == DayOfWeek.Friday || targetDate.DayOfWeek == DayOfWeek.Saturday),
                    capacity = room.Capacity,
                    hasSeaView = room.HasSeaView
                };

                var mlResp = await _mlClient.PredictAsync(mlReq);
                if (mlResp != null)
                {
                    suggestedPrice = (decimal)mlResp.predictedPrice;
                    modelVersion = mlResp.modelVersion;
                    priceSource = "ml_model";
                    var rounded2 = Math.Round(suggestedPrice, 2);
                    return new PricingMvp.Application.DTOs.SuggestedPriceResultDto
                    {
                        SuggestedPrice = rounded2,
                        BasePriceUsed = basePrice,
                        AvgOccupancy = avgOccupancy,
                        HadHistory = hadHistory,
                        PriceSource = priceSource,
                        ModelVersion = modelVersion
                    };
                }
            }

            // Redondear a 2 decimales
            var rounded = Math.Round(suggestedPrice, 2);

            // Determinar priceSource para diagnóstico
            if (room.BasePrice > 0)
                priceSource = "base_price";
            else if (recentHistory.Any())
                priceSource = "room_history";
            else if (hotelHistory.Any())
                priceSource = "hotel_history";
            else
                priceSource = "default";

            return new PricingMvp.Application.DTOs.SuggestedPriceResultDto
            {
                SuggestedPrice = rounded,
                BasePriceUsed = basePrice,
                AvgOccupancy = avgOccupancy,
                HadHistory = hadHistory,
                PriceSource = priceSource,
                ModelVersion = modelVersion
            };
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

            // Además actualizar el precio base de la habitación para reflejar el cambio inmediato
            var room = await _context.Rooms.FindAsync(roomId);
            if (room != null)
            {
                room.BasePrice = newPrice;
            }

            await _context.SaveChangesAsync();

            return true;
        }
    }
}
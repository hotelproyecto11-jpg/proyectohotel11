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
            
            // Regla 1: Ajuste por ocupación (más agresivo)
            if (avgOccupancy > 85)
                suggestedPrice *= 1.35m; // Subir 35%
            else if (avgOccupancy > 75)
                suggestedPrice *= 1.25m; // Subir 25%
            else if (avgOccupancy > 65)
                suggestedPrice *= 1.12m; // Subir 12%
            else if (avgOccupancy > 50)
                suggestedPrice *= 1.05m; // Subir 5%
            else if (avgOccupancy < 35)
                suggestedPrice *= 0.80m; // Bajar 20%
            else if (avgOccupancy < 50)
                suggestedPrice *= 0.90m; // Bajar 10%
            
            // Regla 2: Ajuste por día de semana (fin de semana más alto)
            if (targetDate.DayOfWeek == DayOfWeek.Friday || 
                targetDate.DayOfWeek == DayOfWeek.Saturday)
            {
                suggestedPrice *= 1.20m; // Fin de semana +20%
            }
            else if (targetDate.DayOfWeek == DayOfWeek.Sunday)
            {
                suggestedPrice *= 1.10m; // Domingo +10%
            }
            
            // Regla 3: Ajuste por temporada (más variación)
            int month = targetDate.Month;
            if (month == 12 || month == 1) // Navidad y Año Nuevo
                suggestedPrice *= 1.40m; // +40%
            else if (month == 7 || month == 8) // Verano pico
                suggestedPrice *= 1.25m; // +25%
            else if (month >= 11 || month == 2) // Puentes y temporadas intermedias
                suggestedPrice *= 1.15m; // +15%
            else if (month >= 6 && month <= 9) // Verano general
                suggestedPrice *= 1.12m; // +12%
            else if (month >= 3 && month <= 5) // Primavera
                suggestedPrice *= 1.08m; // +8%

            // Regla 4: Ajuste por características de la habitación
            if (room.HasSeaView)
                suggestedPrice *= 1.15m; // Vista al mar +15%
            
            if (room.HasBalcony)
                suggestedPrice *= 1.10m; // Balcón +10%
            
            if (room.Capacity >= 4)
                suggestedPrice *= 1.12m; // Capacidad alta +12%

            // Si no hay historial, aplicar variación basada en ocupación proyectada
            if (!hadHistory)
            {
                // Añadir variación mayor si la ocupación proyectada es alta o baja
                if (avgOccupancy > 75)
                    suggestedPrice *= 1.08m; // +8% adicional por ocupación alta
                else if (avgOccupancy < 45)
                    suggestedPrice *= 0.95m; // -5% adicional por ocupación baja
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
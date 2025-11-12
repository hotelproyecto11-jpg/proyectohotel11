using PricingMvp.Domain.Common;

namespace PricingMvp.Domain.Entities
{
    public class PriceHistory : BaseEntity
    {
        public int RoomId { get; set; }
        public Room Room { get; set; } = null!;
        
        public DateTime Date { get; set; }
        public decimal Price { get; set; } // Precio aplicado ese día
        public int OccupancyPercent { get; set; } // 0-100
        public bool WasWeekend { get; set; }
        public bool HadEvent { get; set; } // ¿Hubo evento local?
        public string? EventDescription { get; set; }
        
        // Para ML
        public decimal? PredictedPrice { get; set; }
        public int? PredictedOccupancy { get; set; }
    }
}
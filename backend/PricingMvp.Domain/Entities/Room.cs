using PricingMvp.Domain.Common;
using PricingMvp.Domain.Enums;

namespace PricingMvp.Domain.Entities
{
    public class Room : BaseEntity
    {
        public int HotelId { get; set; }
        public Hotel Hotel { get; set; } = null!;
        
        public string RoomNumber { get; set; } = string.Empty;
        public RoomType Type { get; set; }
        public decimal BasePrice { get; set; } // Precio base en MXN
        public int Capacity { get; set; } // Personas que caben
        public int Quantity { get; set; } // Cuántas habitaciones de este tipo hay
        
        // Características
        public bool HasBalcony { get; set; }
        public bool HasSeaView { get; set; }
        public int SquareMeters { get; set; }
        
        // Relaciones
        public ICollection<PriceHistory> PriceHistories { get; set; } = new List<PriceHistory>();
    }
}
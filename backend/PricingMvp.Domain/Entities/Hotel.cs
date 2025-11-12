using PricingMvp.Domain.Common;

namespace PricingMvp.Domain.Entities
{
    public class Hotel : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public int Stars { get; set; } // 1-5 estrellas
        public string? Description { get; set; }
        
        // Relación: Un hotel tiene muchas habitaciones
        public ICollection<Room> Rooms { get; set; } = new List<Room>();
    }
}
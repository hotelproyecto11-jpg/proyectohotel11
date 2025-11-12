namespace PricingMvp.Application.DTOs
{
    public class CreateRoomDto
    {
        public int HotelId { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public int Capacity { get; set; }
        public int Quantity { get; set; }
        public bool HasBalcony { get; set; }
        public bool HasSeaView { get; set; }
        public int SquareMeters { get; set; }
    }
    
    public class UpdateRoomDto
    {
        public decimal BasePrice { get; set; }
        public int Capacity { get; set; }
        public bool HasBalcony { get; set; }
        public bool HasSeaView { get; set; }
    }
}
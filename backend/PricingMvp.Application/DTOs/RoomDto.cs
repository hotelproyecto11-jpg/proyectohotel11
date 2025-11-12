namespace PricingMvp.Application.DTOs
{
    public class RoomDto
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public decimal? SuggestedPrice { get; set; }
        public int Capacity { get; set; }
        public string HotelName { get; set; } = string.Empty;
    }
}
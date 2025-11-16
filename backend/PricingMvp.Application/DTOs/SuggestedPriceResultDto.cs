namespace PricingMvp.Application.DTOs
{
    public class SuggestedPriceResultDto
    {
        public decimal SuggestedPrice { get; set; }
        public decimal BasePriceUsed { get; set; }
        public double AvgOccupancy { get; set; }
        public bool HadHistory { get; set; }
        public string PriceSource { get; set; } = string.Empty; // "room_history"|"hotel_history"|"base_price"|"default"|"ml_model"
        public string? ModelVersion { get; set; }
    }
}

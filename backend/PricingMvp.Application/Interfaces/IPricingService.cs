namespace PricingMvp.Application.Interfaces
{
    public interface IPricingService
    {
        Task<decimal> CalculateSuggestedPriceAsync(int roomId, DateTime targetDate);
        Task<bool> ApplySuggestedPriceAsync(int roomId, decimal newPrice, DateTime effectiveDate);
    }
}
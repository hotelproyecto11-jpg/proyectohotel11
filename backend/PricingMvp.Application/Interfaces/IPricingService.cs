namespace PricingMvp.Application.Interfaces
{
    using PricingMvp.Application.DTOs;

    public interface IPricingService
    {
        Task<SuggestedPriceResultDto> CalculateSuggestedPriceAsync(int roomId, DateTime targetDate);
        Task<bool> ApplySuggestedPriceAsync(int roomId, decimal newPrice, DateTime effectiveDate);
    }
}
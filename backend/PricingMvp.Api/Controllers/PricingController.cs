using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PricingMvp.Application.Interfaces;

namespace PricingMvp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PricingController : ControllerBase
    {
        private readonly IPricingService _pricingService;
        
        public PricingController(IPricingService pricingService)
        {
            _pricingService = pricingService;
        }
        
        // GET: api/pricing/suggest/5?date=2025-12-25
        [HttpGet("suggest/{roomId}")]
        public async Task<ActionResult<decimal>> GetSuggestedPrice(
            int roomId, 
            [FromQuery] DateTime? date = null)
        {
            try
            {
                var targetDate = date ?? DateTime.UtcNow.AddDays(1);
                var suggestedPrice = await _pricingService.CalculateSuggestedPriceAsync(roomId, targetDate);
                
                return Ok(new 
                { 
                    roomId = roomId,
                    targetDate = targetDate.ToString("yyyy-MM-dd"),
                    suggestedPrice = suggestedPrice,
                    currency = "MXN"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        
        // POST: api/pricing/apply
        [HttpPost("apply")]
        [Authorize(Roles = "Admin,RevenueManager")]
        public async Task<ActionResult> ApplySuggestedPrice([FromBody] ApplyPriceDto dto)
        {
            try
            {
                var success = await _pricingService.ApplySuggestedPriceAsync(
                    dto.RoomId, 
                    dto.NewPrice, 
                    dto.EffectiveDate);
                
                if (success)
                    return Ok(new { message = "Precio aplicado exitosamente" });
                
                return BadRequest(new { message = "Error al aplicar precio" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
    
    public class ApplyPriceDto
    {
        public int RoomId { get; set; }
        public decimal NewPrice { get; set; }
        public DateTime EffectiveDate { get; set; }
    }
}
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
        
        // GET: /api/pricing/suggest/{roomId} - Obtiene precio sugerido para una habitación en una fecha
        [AllowAnonymous]
        [HttpGet("suggest/{roomId}")]
        public async Task<ActionResult> GetSuggestedPrice(
            int roomId,
            [FromQuery] DateTime? date = null)
        {
            try
            {
                var targetDate = date ?? DateTime.UtcNow.AddDays(1);
                var result = await _pricingService.CalculateSuggestedPriceAsync(roomId, targetDate);

                return Ok(new
                {
                    roomId = roomId,
                    targetDate = targetDate.ToString("yyyy-MM-dd"),
                    suggestedPrice = result.SuggestedPrice,
                    currency = "MXN",
                    basePriceUsed = result.BasePriceUsed,
                    avgOccupancy = result.AvgOccupancy,
                    hadHistory = result.HadHistory,
                    priceSource = result.PriceSource
                });
            }
            catch (Exception ex)
            {
                // Devolver un mensaje claro para que el frontend pueda mostrarlo
                return BadRequest(new { message = $"No se pudo calcular la sugerencia: {ex.Message}" });
            }
        }
        
        // POST: /api/pricing/apply - Aplica un precio sugerido a una habitación
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
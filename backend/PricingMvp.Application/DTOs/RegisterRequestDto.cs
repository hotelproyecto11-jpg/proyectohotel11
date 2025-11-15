namespace PricingMvp.Application.DTOs
{
    public class RegisterRequestDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        // Optional: desired role name (Admin, RevenueManager, GerenteComercial, StaffOperativo)
        public string? Role { get; set; }
    }
}

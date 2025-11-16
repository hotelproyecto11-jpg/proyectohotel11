using System.ComponentModel.DataAnnotations;

namespace PricingMvp.Application.DTOs
{
    public class RegisterRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [MinLength(2, ErrorMessage = "El nombre completo es requerido")]
        public string FullName { get; set; } = string.Empty;

        // NOTE: Role is intentionally removed from the public registration DTO.
        // Roles must be assigned by an Admin via the Users management endpoints.
    }
}

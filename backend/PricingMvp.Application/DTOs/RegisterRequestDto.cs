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

        public int? HotelId { get; set; }

        // NOTA: El rol se removió intencionalmente del DTO de registro público.
        // Los roles debe ser asignados por un Admin a través de los endpoints de gestión de usuarios.
    }
}

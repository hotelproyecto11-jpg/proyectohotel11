using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PricingMvp.Application.Interfaces;
using System.Security.Claims;

namespace PricingMvp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private readonly IApplicationDbContext _context;

        public UsersController(IApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /api/users - Obtiene lista de usuarios (filtra por hotel si es admin regular)
        [HttpGet]
        public async Task<ActionResult> GetUsers()
        {
            // Obtener email del usuario autenticado
            var currentUserEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            
            // Si es admin@pricingmvp.com, ver todos los usuarios
            // Si es otro admin, solo ver usuarios del mismo hotel
            var query = _context.Users.AsQueryable();
            
            if (currentUserEmail != "admin@pricingmvp.com")
            {
                // Obtener HotelId del admin actual
                var currentAdmin = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == currentUserEmail);
                
                if (currentAdmin?.HotelId == null)
                    return BadRequest(new { message = "El admin no está asignado a un hotel" });
                
                query = query.Where(u => u.HotelId == currentAdmin.HotelId);
            }
            
            var users = await query
                .Select(u => new
                {
                    u.Id,
                    u.Email,
                    u.FullName,
                    Role = u.Role.ToString(),
                    u.IsActive,
                    u.HotelId,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();

            return Ok(users);
        }

        // GET: /api/users/{id} - Obtiene un usuario específico por ID
        [HttpGet("{id}")]
        public async Task<ActionResult> GetUser(int id)
        {
            var user = await _context.Users
                .Where(u => u.Id == id)
                .Select(u => new
                {
                    u.Id,
                    u.Email,
                    u.FullName,
                    Role = u.Role.ToString(),
                    u.IsActive,
                    CreatedAt = u.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound(new { message = "Usuario no encontrado" });

            return Ok(user);
        }

        // PUT: /api/users/{id} - Actualiza los datos de un usuario
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateUser(int id, [FromBody] UpdateUserDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { message = "Usuario no encontrado" });

            if (!string.IsNullOrWhiteSpace(dto.FullName))
                user.FullName = dto.FullName;

            if (!string.IsNullOrWhiteSpace(dto.Email))
                user.Email = dto.Email;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Usuario actualizado" });
        }

        // PATCH: api/users/{id}/toggle-active
        [HttpPatch("{id}/toggle-active")]
        public async Task<ActionResult> ToggleUserActive(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { message = "Usuario no encontrado" });

            user.IsActive = !user.IsActive;
            await _context.SaveChangesAsync();

            var status = user.IsActive ? "activado" : "desactivado";
            return Ok(new { message = $"Usuario {status}", isActive = user.IsActive });
        }

        // DELETE: /api/users/{id} - Elimina un usuario (solo Admin)
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { message = "Usuario no encontrado" });

            // Evitar eliminar al último Admin
            var otherAdmins = await _context.Users
                .Where(u => u.Id != id && u.Role == PricingMvp.Domain.Enums.UserRole.Admin)
                .CountAsync();

            if (user.Role == PricingMvp.Domain.Enums.UserRole.Admin && otherAdmins == 0)
                return BadRequest(new { message = "No se puede eliminar el último Admin" });

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Usuario eliminado" });
        }

        // PATCH: api/users/{id}/role
        // Admin only: asignar rol a un usuario
        [HttpPatch("{id}/role")]
        public async Task<ActionResult> SetUserRole(int id, [FromBody] RoleAssignmentDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Role))
                return BadRequest(new { message = "Role es requerido" });

            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { message = "Usuario no encontrado" });

            if (!Enum.TryParse<PricingMvp.Domain.Enums.UserRole>(dto.Role, true, out var parsed))
                return BadRequest(new { message = "Role inválido" });

            // If demoting an Admin, ensure there is at least one other Admin remaining
            if (user.Role == PricingMvp.Domain.Enums.UserRole.Admin && parsed != PricingMvp.Domain.Enums.UserRole.Admin)
            {
                var otherAdmins = await _context.Users
                    .Where(u => u.Id != id && u.Role == PricingMvp.Domain.Enums.UserRole.Admin)
                    .CountAsync();

                if (otherAdmins == 0)
                    return BadRequest(new { message = "No se puede demotar al último Admin" });
            }

            user.Role = parsed;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Role actualizado", role = user.Role.ToString() });
        }

        // POST: /api/users - Crea un usuario nuevo (solo Admin puede crear con rol asignado)
        [HttpPost]
        public async Task<ActionResult> CreateUser([FromBody] AdminCreateUserDto dto)
        {
            if (dto == null)
                return BadRequest(new { message = "Payload inválido" });

            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password) || string.IsNullOrWhiteSpace(dto.FullName) || string.IsNullOrWhiteSpace(dto.Role))
                return BadRequest(new { message = "Email, Password, FullName y Role son requeridos" });

            // Opcional: mantener la misma restricción de dominio que el registro público
            var allowedDomain = "@posadas.com";
            if (!dto.Email.EndsWith(allowedDomain, StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { message = $"Solo se permiten registros con el dominio '{allowedDomain}'" });

            var exists = await _context.Users.AnyAsync(u => u.Email == dto.Email);
            if (exists)
                return BadRequest(new { message = "Usuario ya existe" });

            if (!Enum.TryParse<PricingMvp.Domain.Enums.UserRole>(dto.Role, true, out var parsed))
                return BadRequest(new { message = "Role inválido" });

            var user = new Domain.Entities.User
            {
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                FullName = dto.FullName,
                Role = parsed,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Usuario creado", email = user.Email, role = user.Role.ToString() });
        }

        // PATCH: api/users/{id}/hotel
        [HttpPatch("{id}/hotel")]
        public async Task<ActionResult> ChangeUserHotel(int id, [FromBody] ChangeUserHotelDto dto)
        {
            if (dto == null || !dto.HotelId.HasValue)
                return BadRequest(new { message = "HotelId es requerido" });

            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { message = "Usuario no encontrado" });

            // Verificar que el hotel existe
            var hotel = await _context.Hotels.FindAsync(dto.HotelId.Value);
            if (hotel == null)
                return NotFound(new { message = "Hotel no encontrado" });

            // Si el admin actual no es admin@pricingmvp.com, solo puede cambiar usuarios de su hotel
            var currentUserEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            if (currentUserEmail != "admin@pricingmvp.com")
            {
                var currentAdmin = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == currentUserEmail);

                // El admin solo puede cambiar usuarios del mismo hotel
                if (user.HotelId != currentAdmin?.HotelId)
                    return Forbid();

                // El admin solo puede asignar usuarios a su propio hotel
                if (dto.HotelId.Value != currentAdmin.HotelId)
                    return BadRequest(new { message = "Solo puedes asignar usuarios a tu propio hotel" });
            }

            user.HotelId = dto.HotelId.Value;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Hotel del usuario actualizado", hotelId = user.HotelId });
        }
    }

    public class UpdateUserDto
    {
        public string? Email { get; set; }
        public string? FullName { get; set; }
    }

    public class RoleAssignmentDto
    {
        public string Role { get; set; } = string.Empty;
    }

    public class AdminCreateUserDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    public class ChangeUserHotelDto
    {
        public int? HotelId { get; set; }
    }
}

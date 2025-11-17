using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PricingMvp.Application.Interfaces;

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

        // GET: api/users
        [HttpGet]
        public async Task<ActionResult> GetUsers()
        {
            var users = await _context.Users
                .Select(u => new
                {
                    u.Id,
                    u.Email,
                    u.FullName,
                    Role = u.Role.ToString(),
                    u.IsActive,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();

            return Ok(users);
        }

        // GET: api/users/{id}
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

        // PUT: api/users/{id}
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

        // DELETE: api/users/{id}
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

        // POST: api/users
        // Admin only: crear usuario (con role asignado)
        [HttpPost]
        public async Task<ActionResult> CreateUser([FromBody] AdminCreateUserDto dto)
        {
            if (dto == null)
                return BadRequest(new { message = "Payload inválido" });

            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password) || string.IsNullOrWhiteSpace(dto.FullName) || string.IsNullOrWhiteSpace(dto.Role))
                return BadRequest(new { message = "Email, Password, FullName y Role son requeridos" });

            // Optional: keep same domain restriction as public register
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
}

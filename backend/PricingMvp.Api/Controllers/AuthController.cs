using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using PricingMvp.Application.DTOs;
using PricingMvp.Application.Interfaces;

namespace PricingMvp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        
        public AuthController(IApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        
        // POST: /api/auth/login - Autentica un usuario y retorna JWT token
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
        {
            // 1. Buscar usuario activo por email
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive);
            
            if (user == null)
                return Unauthorized(new { message = "Usuario o contraseña incorrectos" });
            
            // 2. Verificar contraseña con BCrypt
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return Unauthorized(new { message = "Usuario o contraseña incorrectos" });
            
            // 3. Generar JWT token con claims del usuario
            var token = GenerateJwtToken(user);
            
            return Ok(new LoginResponseDto
            {
                Token = token,
                Email = user.Email,
                Role = user.Role.ToString(),
                FullName = user.FullName,
                HotelId = user.HotelId
            });
        }

        // POST: /api/auth/register - Registra un nuevo usuario (solo dominios corporativos)
        [HttpPost("register")]
        public async Task<ActionResult> Register([FromBody] PricingMvp.Application.DTOs.RegisterRequestDto request)
        {
            // Validar modelo (anotaciones de datos)
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Solo permitir registros con dominio corporativo
            var allowedDomain = "@posadas.com";
            if (!request.Email.EndsWith(allowedDomain, StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { message = $"Solo se permiten registros con el dominio '{allowedDomain}'" });

            // Verificar que el usuario no exista ya
            var existing = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (existing != null)
                return BadRequest(new { message = "Usuario ya existe" });

            // Asignar rol por defecto - Solo admin puede cambiar después
            Domain.Enums.UserRole role = Domain.Enums.UserRole.StaffOperativo;

            var user = new Domain.Entities.User
            {
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                FullName = request.FullName,
                Role = role,
                IsActive = true,
                HotelId = request.HotelId
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Usuario creado", email = user.Email, role = user.Role.ToString() });
        }
        
        private string GenerateJwtToken(Domain.Entities.User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured")));
            
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            
            var claims = new[]
                 {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(ClaimTypes.Name, user.FullName)
            };
            
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    Convert.ToDouble(_configuration["Jwt:ExpiresInMinutes"])),
                signingCredentials: credentials
            );
            
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
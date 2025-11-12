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
        
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
        {
            // 1. Buscar usuario
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive);
            
            if (user == null)
                return Unauthorized(new { message = "Usuario o contraseña incorrectos" });
            
            // 2. Verificar contraseña (en MVP simplificado)
            // En producción usa BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash)
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return Unauthorized(new { message = "Usuario o contraseña incorrectos" });
            
            // 3. Generar JWT token
            var token = GenerateJwtToken(user);
            
            return Ok(new LoginResponseDto
            {
                Token = token,
                Email = user.Email,
                Role = user.Role.ToString(),
                FullName = user.FullName
            });
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
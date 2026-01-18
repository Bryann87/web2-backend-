using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using academia.Data;
using academia.Models;
using academia.Services;

namespace academia.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AcademiaContext _context;
        private readonly IConfiguration _config;
        private readonly IPasswordService _passwordService;

        public AuthController(AcademiaContext context, IConfiguration config, IPasswordService passwordService)
        {
            _context = context;
            _config = config;
            _passwordService = passwordService;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest login)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { message = "Datos de entrada inválidos", errors = ModelState });

                // Buscar persona por correo (sin filtrar por activo primero para dar mensaje específico)
                var user = _context.Personas.FirstOrDefault(x => x.Correo == login.Email);
                
                if (user == null)
                    return Unauthorized(new { message = "Credenciales inválidas." });

                // Verificar si el usuario está activo
                if (!user.Activo)
                    return Unauthorized(new { message = "Usuario desactivado. Contacte al administrador." });

                // Solo administradores y profesores pueden hacer login
                if (user.Rol != "administrador" && user.Rol != "profesor")
                    return Unauthorized(new { message = "No tiene permisos para acceder al sistema." });

                // Verificar contraseña
                if (string.IsNullOrEmpty(user.Contrasena))
                    return Unauthorized(new { message = "Usuario sin contraseña configurada. Contacte al administrador." });

                if (!_passwordService.VerifyPassword(login.Password, user.Contrasena))
                    return Unauthorized(new { message = "Credenciales inválidas." });

                var token = GenerateJwtToken(user);

                return Ok(new
                {
                    token,
                    nombre = user.Nombre,
                    apellido = user.Apellido,
                    email = user.Correo,
                    rol = user.Rol,
                    esProfesor = user.Rol == "profesor",
                    esAdmin = user.Rol == "administrador",
                    idPersona = user.IdPersona
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en login: {ex.Message}");
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
            }
        }

        private string GenerateJwtToken(Persona user)
        {
            var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") 
                           ?? _config["Jwt:SecretKey"];
            
            if (string.IsNullOrEmpty(secretKey))
                throw new Exception("Falta JWT_SECRET_KEY en variables de entorno o configuración");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Correo ?? ""),
                new Claim(ClaimTypes.NameIdentifier, user.IdPersona.ToString()),
                new Claim("nombre_completo", user.NombreCompleto),
                new Claim("rol", user.Rol),
                new Claim("tipo_persona", user.Rol), // Para grupos de SignalR
                new Claim("id", user.IdPersona.ToString()), // Para identificar usuario en SignalR
                new Claim("email", user.Correo ?? ""),
                new Claim(JwtRegisteredClaimNames.Sub, user.IdPersona.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(2),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] Persona persona)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Datos de entrada inválidos", errors = ModelState });

            if (!string.IsNullOrEmpty(persona.Correo) && _context.Personas.Any(p => p.Correo == persona.Correo))
                return BadRequest(new { message = "El correo electrónico ya está registrado" });

            persona.Contrasena = _passwordService.HashPassword(persona.Contrasena);

            try
            {
                _context.Personas.Add(persona);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Usuario registrado exitosamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
            }
        }

        [HttpPost("generate-hash")]
        public IActionResult GenerateHash([FromBody] GenerateHashRequest request)
        {
            string newHash = _passwordService.HashPassword(request.Password);
            return Ok(new { 
                password = request.Password,
                newHash = newHash,
                verification = _passwordService.VerifyPassword(request.Password, newHash)
            });
        }

        public class GenerateHashRequest
        {
            public string Password { get; set; } = string.Empty;
        }
    }
}

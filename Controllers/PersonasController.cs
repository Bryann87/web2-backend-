using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using academia.Data;
using academia.Models;
using academia.DTOs;
using academia.Services;

namespace academia.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PersonasController : ControllerBase
    {
        private readonly AcademiaContext _context;
        private readonly IMappingService _mappingService;
        private readonly IPasswordService _passwordService;

        public PersonasController(AcademiaContext context, IMappingService mappingService, IPasswordService passwordService)
        {
            _context = context;
            _mappingService = mappingService;
            _passwordService = passwordService;
        }

        // GET: api/Personas
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<PersonaDto>>>> GetPersonas(
            [FromQuery] PaginationParams pagination,
            [FromQuery] string? rol = null,
            [FromQuery] bool? activo = null,
            [FromQuery] string? busqueda = null)
        {
            try
            {
                var query = _context.Personas.AsQueryable();

                if (!string.IsNullOrEmpty(rol))
                    query = query.Where(p => p.Rol == rol);
                
                if (activo.HasValue)
                    query = query.Where(p => p.Activo == activo.Value);

                if (!string.IsNullOrEmpty(busqueda))
                    query = query.Where(p => 
                        p.Nombre.Contains(busqueda) || 
                        p.Apellido.Contains(busqueda) ||
                        (p.Correo != null && p.Correo.Contains(busqueda)) ||
                        (p.Cedula != null && p.Cedula.Contains(busqueda)));

                var totalRecords = await query.CountAsync();
                
                var personas = await query
                    .Include(p => p.EstudianteRepresentado)
                    .OrderBy(p => p.Nombre)
                    .Skip(pagination.Skip)
                    .Take(pagination.PageSize)
                    .ToListAsync();

                var personasDto = personas.Select(p => _mappingService.ToDto(p));

                var paginatedResponse = new PaginatedResponse<PersonaDto>
                {
                    Data = personasDto,
                    TotalRecords = totalRecords,
                    Page = pagination.Page,
                    PageSize = pagination.PageSize
                };

                return Ok(ApiResponse<PaginatedResponse<PersonaDto>>.SuccessResponse(paginatedResponse, "Personas obtenidas exitosamente"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PaginatedResponse<PersonaDto>>.ErrorResponse("Error interno del servidor", ex.Message));
            }
        }

        // GET: api/Personas/estudiantes
        [HttpGet("estudiantes")]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<PersonaDto>>>> GetEstudiantes([FromQuery] PaginationParams pagination)
        {
            return await GetPersonasByRol("estudiante", pagination);
        }

        // GET: api/Personas/profesores
        [HttpGet("profesores")]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<PersonaDto>>>> GetProfesores([FromQuery] PaginationParams pagination)
        {
            return await GetPersonasByRol("profesor", pagination);
        }

        // GET: api/Personas/representantes
        [HttpGet("representantes")]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<PersonaDto>>>> GetRepresentantes([FromQuery] PaginationParams pagination)
        {
            return await GetPersonasByRol("representante", pagination);
        }

        // GET: api/Personas/administradores
        [HttpGet("administradores")]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<PersonaDto>>>> GetAdministradores([FromQuery] PaginationParams pagination)
        {
            return await GetPersonasByRol("administrador", pagination);
        }

        private async Task<ActionResult<ApiResponse<PaginatedResponse<PersonaDto>>>> GetPersonasByRol(string rol, PaginationParams pagination)
        {
            try
            {
                var query = _context.Personas.Where(p => p.Rol == rol && p.Activo);
                var totalRecords = await query.CountAsync();
                
                var personas = await query
                    .Include(p => p.EstudianteRepresentado)
                    .OrderBy(p => p.Nombre)
                    .Skip(pagination.Skip)
                    .Take(pagination.PageSize)
                    .ToListAsync();

                var personasDto = personas.Select(p => _mappingService.ToDto(p));

                var paginatedResponse = new PaginatedResponse<PersonaDto>
                {
                    Data = personasDto,
                    TotalRecords = totalRecords,
                    Page = pagination.Page,
                    PageSize = pagination.PageSize
                };

                return Ok(ApiResponse<PaginatedResponse<PersonaDto>>.SuccessResponse(paginatedResponse, $"{rol}s obtenidos exitosamente"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PaginatedResponse<PersonaDto>>.ErrorResponse("Error interno del servidor", ex.Message));
            }
        }

        // GET: api/Personas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<PersonaDto>>> GetPersona(int id)
        {
            try
            {
                var persona = await _context.Personas
                    .Include(p => p.EstudianteRepresentado)
                    .FirstOrDefaultAsync(p => p.IdPersona == id);

                if (persona == null)
                    return NotFound(ApiResponse<PersonaDto>.ErrorResponse("Persona no encontrada"));

                var personaDto = _mappingService.ToDto(persona);
                return Ok(ApiResponse<PersonaDto>.SuccessResponse(personaDto, "Persona obtenida exitosamente"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PersonaDto>.ErrorResponse("Error interno del servidor", ex.Message));
            }
        }

        // GET: api/Personas/estudiante/5/representantes
        [HttpGet("estudiante/{idEstudiante}/representantes")]
        public async Task<ActionResult<ApiResponse<IEnumerable<PersonaDto>>>> GetRepresentantesDeEstudiante(int idEstudiante)
        {
            try
            {
                var representantes = await _context.Personas
                    .Where(p => p.Rol == "representante" && p.IdEstudianteRepresentado == idEstudiante && p.Activo)
                    .ToListAsync();

                var representantesDto = representantes.Select(r => _mappingService.ToDto(r));
                return Ok(ApiResponse<IEnumerable<PersonaDto>>.SuccessResponse(representantesDto, "Representantes obtenidos exitosamente"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<PersonaDto>>.ErrorResponse("Error interno del servidor", ex.Message));
            }
        }

        // POST: api/Personas
        [HttpPost]
        public async Task<ActionResult<ApiResponse<PersonaDto>>> PostPersona(PersonaCreateDto personaDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<PersonaDto>.ErrorResponse("Datos de entrada inválidos", ModelState));

            // Verificar si el email ya existe (solo si tiene correo)
            if (!string.IsNullOrEmpty(personaDto.Correo) && 
                await _context.Personas.AnyAsync(p => p.Correo == personaDto.Correo))
                return BadRequest(ApiResponse<PersonaDto>.ErrorResponse("El correo electrónico ya está registrado"));

            // Verificar cédula única si se proporciona
            if (!string.IsNullOrEmpty(personaDto.Cedula) && 
                await _context.Personas.AnyAsync(p => p.Cedula == personaDto.Cedula))
                return BadRequest(ApiResponse<PersonaDto>.ErrorResponse("La cédula ya está registrada"));

            var rolLower = personaDto.Rol?.ToLower();
            
            // Validar que estudiantes y representantes no pueden tener contraseña
            if ((rolLower == "estudiante" || rolLower == "representante") && !string.IsNullOrEmpty(personaDto.Contrasena))
                return BadRequest(ApiResponse<PersonaDto>.ErrorResponse("Los estudiantes y representantes no pueden tener contraseña. Solo administradores y profesores pueden tener acceso al sistema."));

            // Validar que administradores y profesores DEBEN tener contraseña
            if ((rolLower == "administrador" || rolLower == "profesor") && string.IsNullOrEmpty(personaDto.Contrasena))
                return BadRequest(ApiResponse<PersonaDto>.ErrorResponse("Los administradores y profesores deben tener una contraseña para acceder al sistema."));

            try
            {
                var persona = _mappingService.ToEntity(personaDto);
                
                // Limpiar contraseña para estudiantes y representantes por seguridad
                if (rolLower == "estudiante" || rolLower == "representante")
                    persona.Contrasena = null;
                // Hash de contraseña para admin/profesor (ya validamos que no es null)
                else
                    persona.Contrasena = _passwordService.HashPassword(persona.Contrasena!);

                _context.Personas.Add(persona);
                await _context.SaveChangesAsync();

                var responseDto = _mappingService.ToDto(persona);
                return CreatedAtAction(nameof(GetPersona), new { id = persona.IdPersona }, 
                    ApiResponse<PersonaDto>.SuccessResponse(responseDto, "Persona creada exitosamente"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PersonaDto>.ErrorResponse("Error interno del servidor", ex.Message));
            }
        }

        // PUT: api/Personas/5
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<PersonaDto>>> PutPersona(int id, PersonaUpdateDto personaDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<PersonaDto>.ErrorResponse("Datos de entrada inválidos", ModelState));

            try
            {
                var persona = await _context.Personas.FindAsync(id);
                if (persona == null)
                    return NotFound(ApiResponse<PersonaDto>.ErrorResponse("Persona no encontrada"));

                // Verificar correo único si se está actualizando
                if (!string.IsNullOrEmpty(personaDto.Correo) && personaDto.Correo != persona.Correo &&
                    await _context.Personas.AnyAsync(p => p.Correo == personaDto.Correo))
                    return BadRequest(ApiResponse<PersonaDto>.ErrorResponse("El correo electrónico ya está registrado"));

                _mappingService.UpdateEntity(persona, personaDto);
                await _context.SaveChangesAsync();

                var responseDto = _mappingService.ToDto(persona);
                return Ok(ApiResponse<PersonaDto>.SuccessResponse(responseDto, "Persona actualizada exitosamente"));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Personas.AnyAsync(p => p.IdPersona == id))
                    return NotFound(ApiResponse<PersonaDto>.ErrorResponse("Persona no encontrada"));
                throw;
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PersonaDto>.ErrorResponse("Error interno del servidor", ex.Message));
            }
        }

        // DELETE: api/Personas/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object?>>> DeletePersona(int id)
        {
            try
            {
                var persona = await _context.Personas.FindAsync(id);
                if (persona == null)
                    return NotFound(ApiResponse<object>.ErrorResponse("Persona no encontrada"));

                // Soft delete - solo desactivar
                persona.Activo = false;
                persona.FechaActualizacion = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<object?>.SuccessResponse(null, "Persona eliminada exitosamente"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Error interno del servidor", ex.Message));
            }
        }

        // PUT: api/Personas/5/cambiar-password
        [HttpPut("{id}/cambiar-password")]
        public async Task<ActionResult<ApiResponse<object?>>> CambiarPassword(int id, [FromBody] CambiarPasswordDto dto)
        {
            try
            {
                var persona = await _context.Personas.FindAsync(id);
                if (persona == null)
                    return NotFound(ApiResponse<object>.ErrorResponse("Persona no encontrada"));

                // Solo administradores y profesores pueden tener/cambiar contraseña
                var rolLower = persona.Rol?.ToLower();
                if (rolLower != "administrador" && rolLower != "profesor")
                    return BadRequest(ApiResponse<object>.ErrorResponse("Solo los administradores y profesores pueden tener contraseña."));

                persona.Contrasena = _passwordService.HashPassword(dto.NuevaContrasena);
                persona.FechaActualizacion = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<object?>.SuccessResponse(null, "Contraseña actualizada exitosamente"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Error interno del servidor", ex.Message));
            }
        }

        // PUT: api/Personas/5/toggle-activo
        [HttpPut("{id}/toggle-activo")]
        public async Task<ActionResult<ApiResponse<PersonaDto>>> ToggleActivo(int id)
        {
            try
            {
                var persona = await _context.Personas.FindAsync(id);
                if (persona == null)
                    return NotFound(ApiResponse<PersonaDto>.ErrorResponse("Persona no encontrada"));

                // No permitir desactivar administradores
                if (persona.Rol == "administrador" && persona.Activo)
                    return BadRequest(ApiResponse<PersonaDto>.ErrorResponse("No se puede desactivar un administrador"));

                persona.Activo = !persona.Activo;
                persona.FechaActualizacion = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                var personaDto = _mappingService.ToDto(persona);
                var mensaje = persona.Activo ? "Usuario activado exitosamente" : "Usuario desactivado exitosamente";
                
                return Ok(ApiResponse<PersonaDto>.SuccessResponse(personaDto, mensaje));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PersonaDto>.ErrorResponse("Error interno del servidor", ex.Message));
            }
        }
    }

    public class CambiarPasswordDto
    {
        public string NuevaContrasena { get; set; } = null!;
    }
}

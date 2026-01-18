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
    public class EstilosDanzaController : ControllerBase
    {
        private readonly AcademiaContext _context;
        private readonly IMappingService _mappingService;

        public EstilosDanzaController(AcademiaContext context, IMappingService mappingService)
        {
            _context = context;
            _mappingService = mappingService;
        }

        //http://localhost:5225/api/EstilosDanza
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<EstiloDanzaDto>>>> GetEstilosDanza(
            [FromQuery] PaginationParams pagination,
            [FromQuery] string? nivelDificultad = null,
            [FromQuery] bool? activo = null,
            [FromQuery] string? busqueda = null)
        {
            try
            {
                var query = _context.EstilosDanza.AsQueryable();

                // Filtros
                if (!string.IsNullOrEmpty(nivelDificultad))
                    query = query.Where(e => e.NivelDificultad == nivelDificultad);
                if (activo.HasValue)
                    query = query.Where(e => e.Activo == activo.Value);
                if (!string.IsNullOrEmpty(busqueda))
                    query = query.Where(e => 
                        e.NombreEsti.Contains(busqueda) ||
                        (e.Descripcion != null && e.Descripcion.Contains(busqueda)));

                var totalRecords = await query.CountAsync();
                var estilos = await query
                    .OrderBy(e => e.NombreEsti)
                    .Skip(pagination.Skip)
                    .Take(pagination.PageSize)
                    .ToListAsync();

                var estilosDto = estilos.Select(e => _mappingService.ToDto(e));
                var paginatedResponse = new PaginatedResponse<EstiloDanzaDto>
                {
                    Data = estilosDto,
                    TotalRecords = totalRecords,
                    Page = pagination.Page,
                    PageSize = pagination.PageSize
                };

                return Ok(ApiResponse<PaginatedResponse<EstiloDanzaDto>>.SuccessResponse(paginatedResponse));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PaginatedResponse<EstiloDanzaDto>>.ErrorResponse("Error interno del servidor", ex.Message));
            }
        }

        //http://localhost:5225/api/EstilosDanza/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<EstiloDanzaDto>>> GetEstiloDanza(int id)
        {
            try
            {
                var estiloDanza = await _context.EstilosDanza.FindAsync(id);
                if (estiloDanza == null)
                    return NotFound(ApiResponse<EstiloDanzaDto>.ErrorResponse("Estilo de danza no encontrado"));

                var estiloDto = _mappingService.ToDto(estiloDanza);
                return Ok(ApiResponse<EstiloDanzaDto>.SuccessResponse(estiloDto));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<EstiloDanzaDto>.ErrorResponse("Error interno del servidor", ex.Message));
            }
        }

        //http://localhost:5225/api/EstilosDanza
        [HttpPost]
        public async Task<ActionResult<ApiResponse<EstiloDanzaDto>>> PostEstiloDanza(EstiloDanzaCreateDto estiloDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<EstiloDanzaDto>.ErrorResponse("Datos de entrada inválidos", ModelState));

            try
            {
                var estiloDanza = _mappingService.ToEntity(estiloDto);
                _context.EstilosDanza.Add(estiloDanza);
                await _context.SaveChangesAsync();

                var responseDto = _mappingService.ToDto(estiloDanza);
                return CreatedAtAction(nameof(GetEstiloDanza), new { id = estiloDanza.IdEstilo }, 
                    ApiResponse<EstiloDanzaDto>.SuccessResponse(responseDto, "Estilo de danza creado exitosamente"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<EstiloDanzaDto>.ErrorResponse("Error interno del servidor", ex.Message));
            }
        }

        //http://localhost:5225/api/EstilosDanza/5
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<EstiloDanzaDto>>> PutEstiloDanza(int id, EstiloDanzaUpdateDto estiloDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<EstiloDanzaDto>.ErrorResponse("Datos de entrada inválidos", ModelState));

            try
            {
                var estiloDanza = await _context.EstilosDanza.FindAsync(id);
                if (estiloDanza == null)
                    return NotFound(ApiResponse<EstiloDanzaDto>.ErrorResponse("Estilo de danza no encontrado"));

                _mappingService.UpdateEntity(estiloDanza, estiloDto);
                await _context.SaveChangesAsync();

                var responseDto = _mappingService.ToDto(estiloDanza);
                return Ok(ApiResponse<EstiloDanzaDto>.SuccessResponse(responseDto, "Estilo de danza actualizado exitosamente"));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EstiloDanzaExists(id))
                    return NotFound(ApiResponse<EstiloDanzaDto>.ErrorResponse("Estilo de danza no encontrado"));
                else
                    throw;
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<EstiloDanzaDto>.ErrorResponse("Error interno del servidor", ex.Message));
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object?>>> DeleteEstiloDanza(int id)
        {
            try
            {
                var estiloDanza = await _context.EstilosDanza.FindAsync(id);
                if (estiloDanza == null)
                    return NotFound(ApiResponse<object>.ErrorResponse("Estilo de danza no encontrado"));

                // Verificar si tiene clases asociadas
                var tieneClases = await _context.Clases.AnyAsync(c => c.IdEstilo == id);
                if (tieneClases)
                {
                    return Conflict(ApiResponse<object>.ErrorResponse(
                        "No se puede eliminar este estilo de danza porque tiene clases asociadas. Primero debe eliminar o reasignar las clases.",
                        "HAS_ASSOCIATIONS"));
                }

                _context.EstilosDanza.Remove(estiloDanza);
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<object?>.SuccessResponse(null, "Estilo de danza eliminado exitosamente"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Error interno del servidor", ex.Message));
            }
        }

        private bool EstiloDanzaExists(int id)
        {
            return _context.EstilosDanza.Any(e => e.IdEstilo == id);
        }
    }
}

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
    public class EstadosController : ControllerBase
    {
        private readonly AcademiaContext _context;
        private readonly IMappingService _mappingService;

        public EstadosController(AcademiaContext context, IMappingService mappingService)
        {
            _context = context;
            _mappingService = mappingService;
        }

        [HttpGet]
        //http://localhost:5225/api/Estados
        public async Task<ActionResult<ApiResponse<PaginatedResponse<EstadoDto>>>> Get([FromQuery] PaginationParams pagination)
        {
            try
            {
                var totalRecords = await _context.Estados.CountAsync();
                var estados = await _context.Estados
                    .Include(e => e.Persona)
                    .Skip(pagination.Skip)
                    .Take(pagination.PageSize)
                    .ToListAsync();

                var estadosDto = estados.Select(e => _mappingService.ToDto(e));
                var paginatedResponse = new PaginatedResponse<EstadoDto>
                {
                    Data = estadosDto,
                    TotalRecords = totalRecords,
                    Page = pagination.Page,
                    PageSize = pagination.PageSize
                };

                return Ok(ApiResponse<PaginatedResponse<EstadoDto>>.SuccessResponse(paginatedResponse, "Estados obtenidos exitosamente"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PaginatedResponse<EstadoDto>>.ErrorResponse("Error interno del servidor", ex.Message));
            }
        }

        [HttpGet("{id}")]
        //http://localhost:5225/api/Estados/5
        public async Task<ActionResult<ApiResponse<EstadoDto>>> Get(int id)
        {
            try
            {
                var estado = await _context.Estados
                    .Include(e => e.Persona)
                    .FirstOrDefaultAsync(e => e.IdEstado == id);

                if (estado == null)
                    return NotFound(ApiResponse<EstadoDto>.ErrorResponse("Estado no encontrado"));

                var estadoDto = _mappingService.ToDto(estado);
                return Ok(ApiResponse<EstadoDto>.SuccessResponse(estadoDto, "Estado obtenido exitosamente"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<EstadoDto>.ErrorResponse("Error interno del servidor", ex.Message));
            }
        }

        [HttpPost]
        //http://localhost:5225/api/Estados
        public async Task<ActionResult<ApiResponse<EstadoDto>>> Post(EstadoCreateDto estadoDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<EstadoDto>.ErrorResponse("Datos de entrada inválidos", ModelState));

            try
            {
                var estado = _mappingService.ToEntity(estadoDto);
                _context.Estados.Add(estado);
                await _context.SaveChangesAsync();

                // Recargar con relaciones
                await _context.Entry(estado)
                    .Reference(e => e.Persona)
                    .LoadAsync();

                var responseDto = _mappingService.ToDto(estado);
                return CreatedAtAction(nameof(Get), new { id = estado.IdEstado }, 
                    ApiResponse<EstadoDto>.SuccessResponse(responseDto, "Estado creado exitosamente"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<EstadoDto>.ErrorResponse("Error interno del servidor", ex.Message));
            }
        }

        [HttpPut("{id}")]
        //http://localhost:5225/api/Estados/5
        public async Task<ActionResult<ApiResponse<EstadoDto>>> Put(int id, EstadoUpdateDto estadoDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<EstadoDto>.ErrorResponse("Datos de entrada inválidos", ModelState));

            try
            {
                var estado = await _context.Estados
                    .Include(e => e.Persona)
                    .FirstOrDefaultAsync(e => e.IdEstado == id);

                if (estado == null)
                    return NotFound(ApiResponse<EstadoDto>.ErrorResponse("Estado no encontrado"));

                _mappingService.UpdateEntity(estado, estadoDto);
                await _context.SaveChangesAsync();

                var responseDto = _mappingService.ToDto(estado);
                return Ok(ApiResponse<EstadoDto>.SuccessResponse(responseDto, "Estado actualizado exitosamente"));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Estados.Any(e => e.IdEstado == id))
                    return NotFound(ApiResponse<EstadoDto>.ErrorResponse("Estado no encontrado"));
                else
                    throw;
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<EstadoDto>.ErrorResponse("Error interno del servidor", ex.Message));
            }
        }

        [HttpDelete("{id}")]
        //http://localhost:5225/api/Estados/5
        public async Task<ActionResult<ApiResponse<object?>>> Delete(int id)
        {
            try
            {
                var estado = await _context.Estados.FindAsync(id);
                if (estado == null)
                    return NotFound(ApiResponse<object>.ErrorResponse("Estado no encontrado"));

                _context.Estados.Remove(estado);
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<object?>.SuccessResponse(null, "Estado eliminado exitosamente"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Error interno del servidor", ex.Message));
            }
        }
    }
}

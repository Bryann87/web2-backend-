using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using academia.Data;

namespace academia.Middleware
{
    public class AuthorizationMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthorizationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, AcademiaContext dbContext)
        {
            // Solo procesar si el usuario está autenticado
            if (context.User.Identity?.IsAuthenticated == true)
            {
                // Buscar el rol - puede estar como "rol" o "tipo_persona"
                var tipoPersona = context.User.FindFirst("rol")?.Value 
                               ?? context.User.FindFirst("tipo_persona")?.Value;
                
                // Buscar el ID - puede estar como "sub" (JWT estándar) o ClaimTypes.NameIdentifier
                var userIdClaim = context.User.FindFirst("sub")?.Value 
                               ?? context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int userId))
                {
                    // Si es profesor, agregar claims adicionales
                    if (tipoPersona == "profesor")
                    {
                        var persona = await dbContext.Personas
                            .FirstOrDefaultAsync(p => p.IdPersona == userId && p.Rol == "profesor");

                        if (persona != null)
                        {
                            var identity = context.User.Identity as ClaimsIdentity;
                            if (identity != null)
                            {
                                // Agregar claim con el ID de la persona (profesor)
                                identity.AddClaim(new Claim("id_profesor", persona.IdPersona.ToString()));
                                
                                // Agregar permisos específicos del profesor
                                identity.AddClaim(new Claim("permiso", "ver_mis_clases"));
                                identity.AddClaim(new Claim("permiso", "ver_mis_estudiantes"));
                                identity.AddClaim(new Claim("permiso", "registrar_asistencias"));
                                identity.AddClaim(new Claim("permiso", "ver_mis_asistencias"));
                            }
                        }
                    }
                    // Si es administrador, agregar todos los permisos
                    else if (tipoPersona == "administrador")
                    {
                        var identity = context.User.Identity as ClaimsIdentity;
                        if (identity != null)
                        {
                            identity.AddClaim(new Claim("permiso", "administrador_total"));
                        }
                    }
                }
            }

            await _next(context);
        }
    }
}

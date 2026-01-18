using academia.Models;
using academia.DTOs;

namespace academia.Services
{
    public class MappingService : IMappingService
    {
        // Zona horaria de Ecuador (UTC-5)
        private static readonly TimeZoneInfo EcuadorTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Guayaquil");

        /// <summary>
        /// Convierte una fecha UTC a hora de Ecuador
        /// </summary>
        private static DateTime ToEcuadorTime(DateTime utcDate)
        {
            if (utcDate.Kind == DateTimeKind.Unspecified)
                utcDate = DateTime.SpecifyKind(utcDate, DateTimeKind.Utc);
            return TimeZoneInfo.ConvertTimeFromUtc(utcDate, EcuadorTimeZone);
        }

        /// <summary>
        /// Convierte una fecha de Ecuador a UTC
        /// </summary>
        private static DateTime ToUtcFromEcuador(DateTime ecuadorDate)
        {
            if (ecuadorDate.Kind == DateTimeKind.Unspecified)
                ecuadorDate = DateTime.SpecifyKind(ecuadorDate, DateTimeKind.Unspecified);
            return TimeZoneInfo.ConvertTimeToUtc(ecuadorDate, EcuadorTimeZone);
        }

        // Persona mappings
        public PersonaDto ToDto(Persona persona)
        {
            return new PersonaDto
            {
                IdPersona = persona.IdPersona,
                Nombre = persona.Nombre,
                Apellido = persona.Apellido,
                Telefono = persona.Telefono,
                Correo = persona.Correo,
                Rol = persona.Rol,
                FechaNacimiento = persona.FechaNacimiento,
                Genero = persona.Genero,
                Direccion = persona.Direccion,
                Cedula = persona.Cedula,
                CondicionesMedicas = persona.CondicionesMedicas,
                Especialidad = persona.Especialidad,
                FechaContrato = persona.FechaContrato,
                SalarioBase = persona.SalarioBase,
                Parentesco = persona.Parentesco,
                IdEstudianteRepresentado = persona.IdEstudianteRepresentado,
                NombreEstudianteRepresentado = persona.EstudianteRepresentado?.NombreCompleto,
                Activo = persona.Activo,
                NombreCompleto = persona.NombreCompleto
            };
        }

        public PersonaSimpleDto ToSimpleDto(Persona persona)
        {
            return new PersonaSimpleDto
            {
                IdPersona = persona.IdPersona,
                Nombre = persona.Nombre,
                Apellido = persona.Apellido,
                NombreCompleto = persona.NombreCompleto,
                Rol = persona.Rol
            };
        }

        public Persona ToEntity(PersonaCreateDto dto)
        {
            return new Persona
            {
                Nombre = dto.Nombre,
                Apellido = dto.Apellido,
                Telefono = dto.Telefono,
                Correo = dto.Correo,
                Rol = dto.Rol,
                Contrasena = dto.Contrasena,
                FechaNacimiento = dto.FechaNacimiento.HasValue 
                    ? DateTime.SpecifyKind(dto.FechaNacimiento.Value, DateTimeKind.Utc) : null,
                Genero = dto.Genero,
                Direccion = dto.Direccion,
                Cedula = dto.Cedula,
                CondicionesMedicas = dto.CondicionesMedicas,
                Especialidad = dto.Especialidad,
                FechaContrato = dto.FechaContrato.HasValue 
                    ? DateTime.SpecifyKind(dto.FechaContrato.Value, DateTimeKind.Utc) : null,
                SalarioBase = dto.SalarioBase,
                Parentesco = dto.Parentesco,
                IdEstudianteRepresentado = dto.IdEstudianteRepresentado
            };
        }

        public void UpdateEntity(Persona persona, PersonaUpdateDto dto)
        {
            if (!string.IsNullOrEmpty(dto.Nombre))
                persona.Nombre = dto.Nombre;
            if (!string.IsNullOrEmpty(dto.Apellido))
                persona.Apellido = dto.Apellido;
            if (dto.Telefono != null)
                persona.Telefono = dto.Telefono;
            if (dto.Correo != null)
                persona.Correo = dto.Correo;
            if (!string.IsNullOrEmpty(dto.Rol))
                persona.Rol = dto.Rol;
            if (dto.FechaNacimiento.HasValue)
                persona.FechaNacimiento = DateTime.SpecifyKind(dto.FechaNacimiento.Value, DateTimeKind.Utc);
            if (dto.Genero != null)
                persona.Genero = dto.Genero;
            if (dto.Direccion != null)
                persona.Direccion = dto.Direccion;
            if (dto.Cedula != null)
                persona.Cedula = dto.Cedula;
            if (dto.CondicionesMedicas != null)
                persona.CondicionesMedicas = dto.CondicionesMedicas;
            if (dto.Especialidad != null)
                persona.Especialidad = dto.Especialidad;
            if (dto.FechaContrato.HasValue)
                persona.FechaContrato = DateTime.SpecifyKind(dto.FechaContrato.Value, DateTimeKind.Utc);
            if (dto.SalarioBase.HasValue)
                persona.SalarioBase = dto.SalarioBase;
            if (dto.Parentesco != null)
                persona.Parentesco = dto.Parentesco;
            if (dto.IdEstudianteRepresentado.HasValue)
                persona.IdEstudianteRepresentado = dto.IdEstudianteRepresentado;
            if (dto.Activo.HasValue)
                persona.Activo = dto.Activo.Value;
            
            persona.FechaActualizacion = DateTime.UtcNow;
        }

        // Clase mappings
        public ClaseDto ToDto(Clase clase)
        {
            return new ClaseDto
            {
                IdClase = clase.IdClase,
                NombreClase = clase.NombreClase,
                DiaSemana = clase.DiaSemana,
                Hora = clase.Hora,
                DuracionMinutos = clase.DuracionMinutos,
                CapacidadMax = clase.CapacidadMax,
                PrecioMensuClas = clase.PrecioMensuClas,
                Activa = clase.Activa,
                Profesor = clase.Profesor != null ? ToSimpleDto(clase.Profesor) : null,
                EstiloDanza = clase.EstiloDanza != null ? ToDto(clase.EstiloDanza) : null,
                EstudiantesInscritos = clase.EstudiantesInscritos,
                CuposDisponibles = clase.CuposDisponibles,
                TieneCuposDisponibles = clase.TieneCuposDisponibles
            };
        }

        public ClaseSimpleDto ToSimpleClaseDto(Clase clase)
        {
            return new ClaseSimpleDto
            {
                IdClase = clase.IdClase,
                NombreClase = clase.NombreClase,
                DiaSemana = clase.DiaSemana,
                Hora = clase.Hora
            };
        }

        public Clase ToEntity(ClaseCreateDto dto)
        {
            return new Clase
            {
                NombreClase = dto.NombreClase,
                DiaSemana = dto.DiaSemana,
                Hora = dto.Hora,
                DuracionMinutos = dto.DuracionMinutos,
                CapacidadMax = dto.CapacidadMax,
                PrecioMensuClas = dto.PrecioMensuClas,
                IdProfesor = dto.IdProfesor,
                IdEstilo = dto.IdEstilo
            };
        }

        public void UpdateEntity(Clase clase, ClaseUpdateDto dto)
        {
            clase.NombreClase = dto.NombreClase;
            clase.DiaSemana = dto.DiaSemana;
            clase.Hora = dto.Hora;
            clase.DuracionMinutos = dto.DuracionMinutos;
            clase.CapacidadMax = dto.CapacidadMax;
            clase.PrecioMensuClas = dto.PrecioMensuClas;
            clase.IdProfesor = dto.IdProfesor;
            clase.IdEstilo = dto.IdEstilo;
            clase.Activa = dto.Activa;
        }

        // EstiloDanza mappings
        public EstiloDanzaDto ToDto(EstiloDanza estilo)
        {
            return new EstiloDanzaDto
            {
                IdEstilo = estilo.IdEstilo,
                NombreEsti = estilo.NombreEsti,
                Descripcion = estilo.Descripcion,
                NivelDificultad = estilo.NivelDificultad,
                EdadMinima = estilo.EdadMinima,
                EdadMaxima = estilo.EdadMaxima,
                Activo = estilo.Activo,
                PrecioBase = estilo.PrecioBase
            };
        }

        public EstiloDanza ToEntity(EstiloDanzaCreateDto dto)
        {
            return new EstiloDanza
            {
                NombreEsti = dto.NombreEsti,
                Descripcion = dto.Descripcion,
                NivelDificultad = dto.NivelDificultad,
                EdadMinima = dto.EdadMinima,
                EdadMaxima = dto.EdadMaxima,
                Activo = dto.Activo,
                PrecioBase = dto.PrecioBase
            };
        }

        public void UpdateEntity(EstiloDanza estilo, EstiloDanzaUpdateDto dto)
        {
            estilo.NombreEsti = dto.NombreEsti;
            estilo.Descripcion = dto.Descripcion;
            estilo.NivelDificultad = dto.NivelDificultad;
            estilo.EdadMinima = dto.EdadMinima;
            estilo.EdadMaxima = dto.EdadMaxima;
            estilo.Activo = dto.Activo;
            estilo.PrecioBase = dto.PrecioBase;
        }

        // Asistencia mappings
        public AsistenciaDto ToDto(Asistencia asistencia)
        {
            return new AsistenciaDto
            {
                IdAsist = asistencia.IdAsist,
                FechaAsis = ToEcuadorTime(asistencia.FechaAsis),
                EstadoAsis = asistencia.EstadoAsis,
                Observaciones = asistencia.Observaciones,
                Estudiante = asistencia.Estudiante != null ? ToSimpleDto(asistencia.Estudiante) : null,
                Clase = asistencia.Clase != null ? ToSimpleClaseDto(asistencia.Clase) : null
            };
        }

        public Asistencia ToEntity(AsistenciaCreateDto dto)
        {
            return new Asistencia
            {
                FechaAsis = DateTime.SpecifyKind(dto.FechaAsis, DateTimeKind.Utc),
                EstadoAsis = dto.EstadoAsis,
                Observaciones = dto.Observaciones,
                IdEstudiante = dto.IdEstudiante,
                IdClase = dto.IdClase
            };
        }

        public void UpdateEntity(Asistencia asistencia, AsistenciaUpdateDto dto)
        {
            asistencia.FechaAsis = DateTime.SpecifyKind(dto.FechaAsis, DateTimeKind.Utc);
            asistencia.EstadoAsis = dto.EstadoAsis;
            asistencia.Observaciones = dto.Observaciones;
            asistencia.IdEstudiante = dto.IdEstudiante;
            asistencia.IdClase = dto.IdClase;
        }

        // Cobro mappings
        public CobroDto ToDto(Cobro cobro)
        {
            return new CobroDto
            {
                IdCobro = cobro.IdCobro,
                Monto = cobro.Monto,
                FechaPago = cobro.FechaPago.HasValue ? ToEcuadorTime(cobro.FechaPago.Value) : null,
                FechaVencimiento = cobro.FechaVencimiento.HasValue ? ToEcuadorTime(cobro.FechaVencimiento.Value) : null,
                MetodoPago = cobro.MetodoPago,
                MesCorrespondiente = cobro.MesCorrespondiente,
                EstadoCobro = cobro.EstadoCobro,
                Observaciones = cobro.Observaciones,
                TipoCobro = cobro.TipoCobro,
                AnioCorrespondiente = cobro.AnioCorrespondiente,
                Estudiante = cobro.Estudiante != null ? ToSimpleDto(cobro.Estudiante) : null
            };
        }

        public Cobro ToEntity(CobroCreateDto dto)
        {
            return new Cobro
            {
                Monto = dto.Monto,
                FechaPago = dto.FechaPago.HasValue 
                    ? DateTime.SpecifyKind(dto.FechaPago.Value, DateTimeKind.Utc) : null,
                FechaVencimiento = dto.FechaVencimiento.HasValue 
                    ? DateTime.SpecifyKind(dto.FechaVencimiento.Value, DateTimeKind.Utc) : null,
                MetodoPago = dto.MetodoPago,
                MesCorrespondiente = dto.MesCorrespondiente,
                EstadoCobro = dto.EstadoCobro,
                Observaciones = dto.Observaciones,
                TipoCobro = dto.TipoCobro,
                AnioCorrespondiente = dto.AnioCorrespondiente,
                IdEstudiante = dto.IdEstudiante
            };
        }

        public void UpdateEntity(Cobro cobro, CobroUpdateDto dto)
        {
            cobro.Monto = dto.Monto;
            if (dto.FechaPago.HasValue)
                cobro.FechaPago = DateTime.SpecifyKind(dto.FechaPago.Value, DateTimeKind.Utc);
            if (dto.FechaVencimiento.HasValue)
                cobro.FechaVencimiento = DateTime.SpecifyKind(dto.FechaVencimiento.Value, DateTimeKind.Utc);
            if (dto.MetodoPago != null)
                cobro.MetodoPago = dto.MetodoPago;
            if (dto.MesCorrespondiente != null)
                cobro.MesCorrespondiente = dto.MesCorrespondiente;
            if (dto.EstadoCobro != null)
                cobro.EstadoCobro = dto.EstadoCobro;
            if (dto.Observaciones != null)
                cobro.Observaciones = dto.Observaciones;
            if (dto.TipoCobro != null)
                cobro.TipoCobro = dto.TipoCobro;
            if (dto.AnioCorrespondiente.HasValue)
                cobro.AnioCorrespondiente = dto.AnioCorrespondiente;
        }

        // Inscripcion mappings
        public InscripcionDto ToDto(Inscripcion inscripcion)
        {
            return new InscripcionDto
            {
                IdInsc = inscripcion.IdInsc,
                FechaInsc = ToEcuadorTime(inscripcion.FechaInsc),
                Estado = inscripcion.Estado,
                FechaBaja = inscripcion.FechaBaja.HasValue ? ToEcuadorTime(inscripcion.FechaBaja.Value) : null,
                MotivoBaja = inscripcion.MotivoBaja,
                Estudiante = inscripcion.Estudiante != null ? ToSimpleDto(inscripcion.Estudiante) : null,
                Clase = inscripcion.Clase != null ? ToSimpleClaseDto(inscripcion.Clase) : null
            };
        }

        public Inscripcion ToEntity(InscripcionCreateDto dto)
        {
            return new Inscripcion
            {
                FechaInsc = DateTime.SpecifyKind(dto.FechaInsc, DateTimeKind.Utc),
                Estado = dto.Estado,
                IdEstudiante = dto.IdEstudiante,
                IdClase = dto.IdClase
            };
        }

        public void UpdateEntity(Inscripcion inscripcion, InscripcionUpdateDto dto)
        {
            if (dto.FechaInsc.HasValue)
                inscripcion.FechaInsc = DateTime.SpecifyKind(dto.FechaInsc.Value, DateTimeKind.Utc);
            if (!string.IsNullOrEmpty(dto.Estado))
                inscripcion.Estado = dto.Estado;
            if (dto.FechaBaja.HasValue)
                inscripcion.FechaBaja = DateTime.SpecifyKind(dto.FechaBaja.Value, DateTimeKind.Utc);
            if (dto.MotivoBaja != null)
                inscripcion.MotivoBaja = dto.MotivoBaja;
            if (dto.IdEstudiante.HasValue)
                inscripcion.IdEstudiante = dto.IdEstudiante.Value;
            if (dto.IdClase.HasValue)
                inscripcion.IdClase = dto.IdClase.Value;
        }

        // Estado mappings
        public EstadoDto ToDto(Estado estado)
        {
            return new EstadoDto
            {
                IdEstado = estado.IdEstado,
                FechaCreacion = ToEcuadorTime(estado.FechaCreacion),
                FechaActualizacion = ToEcuadorTime(estado.FechaActualizacion),
                NoAsisto = estado.NoAsisto,
                Retirado = estado.Retirado,
                Activo = estado.Activo,
                Persona = estado.Persona != null ? ToSimpleDto(estado.Persona) : null
            };
        }

        public Estado ToEntity(EstadoCreateDto dto)
        {
            return new Estado
            {
                FechaCreacion = DateTime.SpecifyKind(dto.FechaCreacion, DateTimeKind.Utc),
                FechaActualizacion = DateTime.SpecifyKind(dto.FechaActualizacion, DateTimeKind.Utc),
                NoAsisto = dto.NoAsisto,
                Retirado = dto.Retirado,
                Activo = dto.Activo,
                IdPersona = dto.IdPersona
            };
        }

        public void UpdateEntity(Estado estado, EstadoUpdateDto dto)
        {
            estado.FechaActualizacion = DateTime.SpecifyKind(dto.FechaActualizacion, DateTimeKind.Utc);
            estado.NoAsisto = dto.NoAsisto;
            estado.Retirado = dto.Retirado;
            estado.Activo = dto.Activo;
        }
    }
}

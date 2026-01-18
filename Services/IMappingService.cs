using academia.Models;
using academia.DTOs;

namespace academia.Services
{
    public interface IMappingService
    {
        // Persona mappings
        PersonaDto ToDto(Persona persona);
        PersonaSimpleDto ToSimpleDto(Persona persona);
        Persona ToEntity(PersonaCreateDto dto);
        void UpdateEntity(Persona persona, PersonaUpdateDto dto);

        // Clase mappings
        ClaseDto ToDto(Clase clase);
        ClaseSimpleDto ToSimpleClaseDto(Clase clase);
        Clase ToEntity(ClaseCreateDto dto);
        void UpdateEntity(Clase clase, ClaseUpdateDto dto);

        // EstiloDanza mappings
        EstiloDanzaDto ToDto(EstiloDanza estilo);
        EstiloDanza ToEntity(EstiloDanzaCreateDto dto);
        void UpdateEntity(EstiloDanza estilo, EstiloDanzaUpdateDto dto);

        // Asistencia mappings
        AsistenciaDto ToDto(Asistencia asistencia);
        Asistencia ToEntity(AsistenciaCreateDto dto);
        void UpdateEntity(Asistencia asistencia, AsistenciaUpdateDto dto);

        // Cobro mappings
        CobroDto ToDto(Cobro cobro);
        Cobro ToEntity(CobroCreateDto dto);
        void UpdateEntity(Cobro cobro, CobroUpdateDto dto);

        // Inscripcion mappings
        InscripcionDto ToDto(Inscripcion inscripcion);
        Inscripcion ToEntity(InscripcionCreateDto dto);
        void UpdateEntity(Inscripcion inscripcion, InscripcionUpdateDto dto);

        // Estado mappings
        EstadoDto ToDto(Estado estado);
        Estado ToEntity(EstadoCreateDto dto);
        void UpdateEntity(Estado estado, EstadoUpdateDto dto);
    }
}

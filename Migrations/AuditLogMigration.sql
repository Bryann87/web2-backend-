-- Migración para crear la tabla de auditoría
-- Ejecutar este script si no se usa EF Core Migrations

CREATE TABLE IF NOT EXISTS audit_log (
    id_audit BIGSERIAL PRIMARY KEY,
    tabla_afectada VARCHAR(100) NOT NULL,
    tipo_operacion VARCHAR(20) NOT NULL,
    id_registro VARCHAR(255),
    datos_anteriores TEXT,
    datos_nuevos TEXT,
    campos_modificados TEXT,
    id_usuario INTEGER,
    nombre_usuario VARCHAR(100),
    rol_usuario VARCHAR(50),
    ip_address VARCHAR(50),
    user_agent VARCHAR(500),
    endpoint VARCHAR(200),
    metodo_http VARCHAR(10),
    fecha_operacion TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    duracion_ms BIGINT,
    exitoso BOOLEAN NOT NULL DEFAULT TRUE,
    mensaje_error TEXT,
    CONSTRAINT fk_audit_usuario FOREIGN KEY (id_usuario) REFERENCES persona(id_persona) ON DELETE SET NULL
);

-- Índices para búsquedas eficientes
CREATE INDEX IF NOT EXISTS idx_audit_tabla ON audit_log(tabla_afectada);
CREATE INDEX IF NOT EXISTS idx_audit_fecha ON audit_log(fecha_operacion);
CREATE INDEX IF NOT EXISTS idx_audit_usuario ON audit_log(id_usuario);
CREATE INDEX IF NOT EXISTS idx_audit_tabla_registro ON audit_log(tabla_afectada, id_registro);
CREATE INDEX IF NOT EXISTS idx_audit_tipo_operacion ON audit_log(tipo_operacion);

-- Comentarios de documentación
COMMENT ON TABLE audit_log IS 'Tabla de auditoría que registra todas las operaciones de INSERT, UPDATE y DELETE';
COMMENT ON COLUMN audit_log.tabla_afectada IS 'Nombre de la tabla donde se realizó la operación';
COMMENT ON COLUMN audit_log.tipo_operacion IS 'Tipo de operación: INSERT, UPDATE o DELETE';
COMMENT ON COLUMN audit_log.id_registro IS 'ID del registro afectado';
COMMENT ON COLUMN audit_log.datos_anteriores IS 'JSON con los valores anteriores (para UPDATE y DELETE)';
COMMENT ON COLUMN audit_log.datos_nuevos IS 'JSON con los valores nuevos (para INSERT y UPDATE)';
COMMENT ON COLUMN audit_log.campos_modificados IS 'JSON con la lista de campos que fueron modificados (para UPDATE)';

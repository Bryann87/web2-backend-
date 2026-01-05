-- Script para agregar datos iniciales a la base de datos de la academia (PostgreSQL)
-- IMPORTANTE: Todas las contraseñas de prueba son "123456" (hasheadas con BCrypt)
-- Hash correcto: $2a$12$3NPl6vAvInuxUMOIYSCaCOmFBvTILuBgXVqjc9S/HAX.fQJ4Ozpam

-- Limpiar datos existentes (en orden correcto para evitar errores de FK)
DELETE FROM asistencia;
DELETE FROM cobro;
DELETE FROM inscripcion;
DELETE FROM clase;
DELETE FROM estado;
DELETE FROM persona;
DELETE FROM estilo_danza;

-- 1. Insertar estilos de danza (ids 1-10)
INSERT INTO estilo_danza (nombre_esti, descripcion, nivel_dificultad, edad_minima, edad_maxima, activo, precio_base) VALUES 
('Salsa', 'Baile latino lleno de energía y pasión', 'Principiante', 12, NULL, true, 2500.00),
('Bachata', 'Baile romántico originario de República Dominicana', 'Principiante', 14, NULL, true, 2800.00),
('Merengue', 'Baile tradicional dominicano de ritmo alegre', 'Principiante', 10, NULL, true, 2300.00),
('Reggaeton', 'Baile urbano moderno con movimientos dinámicos', 'Intermedio', 13, NULL, true, 2200.00),
('Hip Hop', 'Danza urbana con estilo libre y expresivo', 'Intermedio', 8, NULL, true, 3000.00),
('Ballet', 'Danza clásica que desarrolla técnica y elegancia', 'Principiante', 5, NULL, true, 3500.00),
('Kizomba', 'Baile sensual de origen africano', 'Intermedio', 16, NULL, true, 2900.00),
('Tango', 'Baile argentino apasionado y técnico', 'Avanzado', 18, NULL, true, 3800.00),
('Flamenco', 'Arte español que combina baile, cante y guitarra', 'Intermedio', 12, NULL, true, 3300.00),
('Jazz', 'Danza moderna con influencias del jazz musical', 'Intermedio', 10, NULL, true, 3100.00);

-- 2. Insertar personas
-- Administrador (id=1)
INSERT INTO persona (nombre, apellido, telefono, correo, rol, contraseña, activo, fecha_creacion, fecha_actualizacion) VALUES 
('Admin', 'Sistema', '809-555-0000', 'admin@academia.com', 'administrador', '$2a$12$3NPl6vAvInuxUMOIYSCaCOmFBvTILuBgXVqjc9S/HAX.fQJ4Ozpam', true, NOW(), NOW())
ON CONFLICT (correo) DO UPDATE SET 
    nombre = EXCLUDED.nombre,
    apellido = EXCLUDED.apellido,
    contraseña = EXCLUDED.contraseña,
    activo = EXCLUDED.activo,
    fecha_actualizacion = NOW();

-- Profesores (ids 2-6)
INSERT INTO persona (nombre, apellido, telefono, correo, rol, especialidad, fecha_contrato, salario_base, contraseña, activo, fecha_creacion, fecha_actualizacion) VALUES 
('María', 'Zambrano', '809-555-0001', 'maria.zambrano@academia.com', 'profesor', 'Salsa y Bachata', '2023-01-15', 35000.00, '$2a$12$3NPl6vAvInuxUMOIYSCaCOmFBvTILuBgXVqjc9S/HAX.fQJ4Ozpam', true, NOW(), NOW()),
('Carlos', 'Macías', '809-555-0002', 'carlos.macias@academia.com', 'profesor', 'Hip Hop y Reggaeton', '2023-02-01', 32000.00, '$2a$12$3NPl6vAvInuxUMOIYSCaCOmFBvTILuBgXVqjc9S/HAX.fQJ4Ozpam', true, NOW(), NOW()),
('Ana', 'Cedeño', '809-555-0003', 'ana.cedeno@academia.com', 'profesor', 'Ballet Clásico', '2023-03-10', 38000.00, '$2a$12$3NPl6vAvInuxUMOIYSCaCOmFBvTILuBgXVqjc9S/HAX.fQJ4Ozpam', true, NOW(), NOW()),
('Luis', 'Moreira', '809-555-0004', 'luis.moreira@academia.com', 'profesor', 'Tango y Flamenco', '2023-04-01', 36000.00, '$2a$12$3NPl6vAvInuxUMOIYSCaCOmFBvTILuBgXVqjc9S/HAX.fQJ4Ozpam', true, NOW(), NOW()),
('Isabella', 'Vera', '809-555-0005', 'isabella.vera@academia.com', 'profesor', 'Kizomba y Jazz', '2023-05-15', 34000.00, '$2a$12$3NPl6vAvInuxUMOIYSCaCOmFBvTILuBgXVqjc9S/HAX.fQJ4Ozpam', true, NOW(), NOW())
ON CONFLICT (correo) DO NOTHING;

-- Estudiantes adultos (ids 7-12)
INSERT INTO persona (nombre, apellido, telefono, correo, rol, fecha_nacimiento, genero, cedula, direccion, condiciones_medicas, activo, fecha_creacion, fecha_actualizacion) VALUES 
('Santiago', 'Quishpe', '809-555-0101', 'santiago.quishpe@email.com', 'estudiante', '1995-05-20', 'M', '001-1234567-8', 'Calle Principal #45, Quito Norte', NULL, true, NOW(), NOW()),
('Valentina', 'Arango', '809-555-0102', 'valentina.arango@email.com', 'estudiante', '2000-08-15', 'F', '001-2345678-9', 'Av. Amazonas #123', 'Alergia a ciertos perfumes', true, NOW(), NOW()),
('Sebastián', 'Cárdenas', '809-555-0103', 'sebastian.cardenas@email.com', 'estudiante', '1998-12-03', 'M', '001-3456789-0', 'Calle Las Orquídeas #78', NULL, true, NOW(), NOW()),
('Camila', 'Ospina', '809-555-0104', 'camila.ospina@email.com', 'estudiante', '2001-03-25', 'F', '001-4567890-1', 'Residencial El Batán', NULL, true, NOW(), NOW()),
('Andrés', 'Jaramillo', '809-555-0105', 'andres.jaramillo@email.com', 'estudiante', '1997-09-10', 'M', '001-5678901-2', 'Calle El Inca #34', 'Lesión previa en rodilla izquierda', true, NOW(), NOW()),
('Mariana', 'Betancourt', '809-555-0110', 'mariana.betancourt@email.com', 'estudiante', '1994-09-22', 'F', '001-6789012-3', 'Av. 6 de Diciembre #567', NULL, true, NOW(), NOW())
ON CONFLICT (correo) DO NOTHING;

-- Estudiantes menores de edad (ids 13-18)
INSERT INTO persona (nombre, apellido, telefono, correo, rol, fecha_nacimiento, genero, direccion, condiciones_medicas, activo, fecha_creacion, fecha_actualizacion) VALUES 
('Luciana', 'Chávez', '809-555-0106', 'luciana.chavez@email.com', 'estudiante', '2012-07-12', 'F', 'Calle Los Ceibos #89', NULL, true, NOW(), NOW()),
('Matías', 'Gutiérrez', '809-555-0107', 'matias.gutierrez@email.com', 'estudiante', '2011-01-08', 'M', 'Urbanización La Carolina', NULL, true, NOW(), NOW()),
('Emiliano', 'Salazar', '809-555-0109', 'emiliano.salazar@email.com', 'estudiante', '2010-04-18', 'M', 'Calle Colón #156', NULL, true, NOW(), NOW()),
('Joaquín', 'Velasco', '809-555-0111', 'joaquin.velasco@email.com', 'estudiante', '2013-06-14', 'M', 'Av. República #234', 'Asma leve', true, NOW(), NOW()),
('Isabella', 'Restrepo', '809-555-0108', 'isabella.restrepo@email.com', 'estudiante', '2009-11-30', 'F', 'Calle Sucre #67', NULL, true, NOW(), NOW()),
('Antonella', 'Zuñiga', '809-555-0112', 'antonella.zuniga@email.com', 'estudiante', '2014-02-28', 'F', 'Residencial Cumbayá', NULL, true, NOW(), NOW())
ON CONFLICT (correo) DO NOTHING;


-- Representantes para estudiantes menores (ids 19-24)
INSERT INTO persona (nombre, apellido, telefono, correo, rol, cedula, direccion, parentesco, id_estudiante_representado, activo, fecha_creacion, fecha_actualizacion) VALUES 
('Patricia', 'Chávez', '809-555-0201', 'patricia.chavez@email.com', 'representante', '001-7890123-4', 'Calle Los Ceibos #89', 'Madre', 13, true, NOW(), NOW()),
('Ricardo', 'Gutiérrez', '809-555-0202', 'ricardo.gutierrez@email.com', 'representante', '001-8901234-5', 'Urbanización La Carolina', 'Padre', 14, true, NOW(), NOW()),
('Adriana', 'Salazar', '809-555-0203', 'adriana.salazar@email.com', 'representante', '001-9012345-6', 'Calle Colón #156', 'Madre', 15, true, NOW(), NOW()),
('Mauricio', 'Velasco', '809-555-0204', 'mauricio.velasco@email.com', 'representante', '001-0123456-7', 'Av. República #234', 'Padre', 16, true, NOW(), NOW()),
('Carolina', 'Restrepo', '809-555-0205', 'carolina.restrepo@email.com', 'representante', '001-1234560-8', 'Calle Sucre #67', 'Madre', 17, true, NOW(), NOW()),
('Jorge', 'Zuñiga', '809-555-0206', 'jorge.zuniga@email.com', 'representante', '001-2345601-9', 'Residencial Cumbayá', 'Padre', 18, true, NOW(), NOW())
ON CONFLICT (correo) DO NOTHING;

-- 3. Insertar clases (ids 1-8)
INSERT INTO clase (nombre_clase, dia_semana, hora, duracion_minutos, capacidad_max, precio_mensu_clas, activa, fecha_inicio, id_profesor, id_estilo) VALUES 
('Salsa Principiantes', 'Lunes', '18:00:00', 60, 20, 2500.00, true, '2025-01-06', 2, 1),
('Salsa Intermedio', 'Miércoles', '19:00:00', 60, 15, 2800.00, true, '2025-01-08', 2, 1),
('Bachata Romántica', 'Martes', '18:00:00', 60, 20, 2800.00, true, '2025-01-07', 2, 2),
('Hip Hop Kids', 'Sábado', '10:00:00', 45, 25, 2500.00, true, '2025-01-11', 3, 5),
('Hip Hop Adultos', 'Jueves', '20:00:00', 60, 20, 3000.00, true, '2025-01-09', 3, 5),
('Ballet Infantil', 'Martes', '16:00:00', 45, 15, 3500.00, true, '2025-01-07', 4, 6),
('Ballet Clásico', 'Jueves', '17:00:00', 60, 12, 3800.00, true, '2025-01-09', 4, 6),
('Tango Argentino', 'Viernes', '19:00:00', 90, 16, 3800.00, true, '2025-01-10', 5, 8),
('Kizomba Sensual', 'Sábado', '18:00:00', 60, 20, 2900.00, true, '2025-01-11', 6, 7),
('Jazz Moderno', 'Miércoles', '17:00:00', 60, 18, 3100.00, true, '2025-01-08', 6, 10);

-- 4. Insertar inscripciones (estudiantes en clases)
INSERT INTO inscripcion (fecha_insc, estado, id_estudiante, id_clase) VALUES 
-- Estudiantes adultos en clases de adultos
('2025-01-06', 'Activa', 7, 1),   -- Santiago en Salsa Principiantes
('2025-01-06', 'Activa', 8, 1),   -- Valentina en Salsa Principiantes
('2025-01-06', 'Activa', 9, 2),   -- Sebastián en Salsa Intermedio
('2025-01-07', 'Activa', 10, 3),  -- Camila en Bachata
('2025-01-07', 'Activa', 7, 3),   -- Santiago también en Bachata
('2025-01-09', 'Activa', 11, 5),  -- Andrés en Hip Hop Adultos
('2025-01-09', 'Activa', 12, 7),  -- Mariana en Ballet Clásico
('2025-01-10', 'Activa', 9, 8),   -- Sebastián en Tango
('2025-01-10', 'Activa', 10, 8),  -- Camila en Tango
('2025-01-11', 'Activa', 8, 9),   -- Valentina en Kizomba
-- Estudiantes menores en clases infantiles
('2025-01-11', 'Activa', 13, 4),  -- Luciana en Hip Hop Kids
('2025-01-11', 'Activa', 14, 4),  -- Matías en Hip Hop Kids
('2025-01-11', 'Activa', 15, 4),  -- Emiliano en Hip Hop Kids
('2025-01-07', 'Activa', 16, 6),  -- Joaquín en Ballet Infantil
('2025-01-07', 'Activa', 17, 6),  -- Isabella en Ballet Infantil
('2025-01-07', 'Activa', 18, 6),  -- Antonella en Ballet Infantil
-- Inscripción cancelada de ejemplo
('2025-01-06', 'Cancelada', 11, 1);  -- Andrés canceló Salsa

-- Actualizar la inscripción cancelada con motivo
UPDATE inscripcion SET fecha_baja = '2025-01-15', motivo_baja = 'Cambio de horario laboral' WHERE id_estudiante = 11 AND id_clase = 1;

-- 5. Insertar estados del sistema
INSERT INTO estado (fecha_creacion, fecha_actualizacion, id_persona, no_asisto, retirado, activo) VALUES 
(NOW(), NOW(), 7, false, false, true),   -- Santiago activo
(NOW(), NOW(), 8, false, false, true),   -- Valentina activa
(NOW(), NOW(), 9, false, false, true),   -- Sebastián activo
(NOW(), NOW(), 10, false, false, true),  -- Camila activa
(NOW(), NOW(), 11, true, false, true),   -- Andrés con inasistencias
(NOW(), NOW(), 12, false, false, true),  -- Mariana activa
(NOW(), NOW(), 13, false, false, true),  -- Luciana activa
(NOW(), NOW(), 14, false, false, true),  -- Matías activo
(NOW(), NOW(), 15, false, false, true),  -- Emiliano activo
(NOW(), NOW(), 16, false, false, true),  -- Joaquín activo
(NOW(), NOW(), 17, false, false, true),  -- Isabella activa
(NOW(), NOW(), 18, false, false, true);  -- Antonella activa


-- ============================================
-- 3. CLASES (IDs 1-8)
-- ============================================
INSERT INTO clase (nombre_clase, dia_semana, hora, duracion_minutos, capacidad_max, precio_mensu_clas, activa, fecha_inicio, id_profesor, id_estilo) VALUES 
-- Clases de María Zambrano (Salsa y Bachata)
('Salsa Principiantes', 'Lunes', '18:00:00', 60, 20, 2500.00, true, '2025-01-06', 2, 1),
('Bachata Romántica', 'Miércoles', '19:00:00', 60, 18, 2800.00, true, '2025-01-08', 2, 2),
-- Clases de Carlos Macías (Hip Hop y Reggaeton)
('Hip Hop Kids', 'Martes', '16:00:00', 45, 15, 3000.00, true, '2025-01-07', 3, 5),
('Reggaeton Fitness', 'Jueves', '20:00:00', 60, 25, 2200.00, true, '2025-01-09', 3, 4),
-- Clases de Ana Cedeño (Ballet)
('Ballet Infantil', 'Sábado', '09:00:00', 60, 12, 3500.00, true, '2025-01-11', 4, 6),
('Ballet Clásico Adultos', 'Sábado', '11:00:00', 90, 15, 4000.00, true, '2025-01-11', 4, 6),
-- Clases de Luis Moreira (Tango y Flamenco)
('Tango Argentino', 'Viernes', '19:00:00', 75, 16, 3800.00, true, '2025-01-10', 5, 8),
-- Clases de Isabella Vera (Kizomba y Jazz)
('Jazz Contemporáneo', 'Miércoles', '17:00:00', 60, 18, 3100.00, true, '2025-01-08', 6, 10);

-- ============================================
-- 4. INSCRIPCIONES (IDs 1-20)
-- ============================================
INSERT INTO inscripcion (fecha_insc, estado, id_estudiante, id_clase) VALUES 
-- Inscripciones a Salsa Principiantes (clase 1)
('2025-01-06', 'Activa', 7, 1),   -- Santiago Quishpe
('2025-01-06', 'Activa', 8, 1),   -- Valentina Arango
('2025-01-07', 'Activa', 9, 1),   -- Sebastián Cárdenas
('2025-01-08', 'Activa', 12, 1),  -- Mariana Betancourt

-- Inscripciones a Bachata Romántica (clase 2)
('2025-01-08', 'Activa', 7, 2),   -- Santiago Quishpe
('2025-01-08', 'Activa', 10, 2),  -- Camila Ospina
('2025-01-09', 'Activa', 13, 2),  -- Luciana Chávez

-- Inscripciones a Hip Hop Kids (clase 3) - Menores
('2025-01-07', 'Activa', 15, 3),  -- Emiliano Salazar
('2025-01-07', 'Activa', 16, 3),  -- Joaquín Velasco
('2025-01-08', 'Activa', 17, 3),  -- Isabella Restrepo

-- Inscripciones a Reggaeton Fitness (clase 4)
('2025-01-09', 'Activa', 8, 4),   -- Valentina Arango
('2025-01-09', 'Activa', 11, 4),  -- Andrés Jaramillo
('2025-01-10', 'Activa', 14, 4),  -- Matías Gutiérrez

-- Inscripciones a Ballet Infantil (clase 5) - Menores
('2025-01-11', 'Activa', 15, 5),  -- Emiliano Salazar
('2025-01-11', 'Activa', 18, 5),  -- Antonella Zuñiga

-- Inscripciones a Ballet Clásico Adultos (clase 6)
('2025-01-11', 'Activa', 10, 6),  -- Camila Ospina
('2025-01-12', 'Activa', 13, 6),  -- Luciana Chávez

-- Inscripciones a Tango Argentino (clase 7)
('2025-01-10', 'Activa', 12, 7),  -- Mariana Betancourt
('2025-01-10', 'Activa', 11, 7),  -- Andrés Jaramillo

-- Inscripciones a Jazz Contemporáneo (clase 8)
('2025-01-08', 'Activa', 14, 8);  -- Matías Gutiérrez

-- ============================================
-- 5. COBROS - Con nuevos campos tipo_cobro y año_correspondiente
-- ============================================

-- Cobros MENSUALES pagados (Enero 2025)
INSERT INTO cobro (monto, fecha_pago, fecha_vencimiento, metodo_pago, mes_correspondiente, estado_cobro, tipo_cobro, año_correspondiente, id_estudiante, fecha_creacion) VALUES 
(2500.00, '2025-01-06', '2025-01-15', 'efectivo', 'Enero', 'pagado', 'mensual', 2025, 7, NOW()),
(2800.00, '2025-01-08', '2025-01-15', 'tarjeta', 'Enero', 'pagado', 'mensual', 2025, 8, NOW()),
(2500.00, '2025-01-07', '2025-01-15', 'transferencia', 'Enero', 'pagado', 'mensual', 2025, 9, NOW()),
(3000.00, '2025-01-07', '2025-01-15', 'efectivo', 'Enero', 'pagado', 'mensual', 2025, 15, NOW()),
(3000.00, '2025-01-07', '2025-01-15', 'tarjeta', 'Enero', 'pagado', 'mensual', 2025, 16, NOW());

-- Cobros MENSUALES pendientes (Febrero 2025)
INSERT INTO cobro (monto, fecha_vencimiento, mes_correspondiente, estado_cobro, tipo_cobro, año_correspondiente, id_estudiante, fecha_creacion) VALUES 
(2500.00, '2025-02-15', 'Febrero', 'pendiente', 'mensual', 2025, 7, NOW()),
(2800.00, '2025-02-15', 'Febrero', 'pendiente', 'mensual', 2025, 8, NOW()),
(2500.00, '2025-02-15', 'Febrero', 'pendiente', 'mensual', 2025, 9, NOW()),
(2800.00, '2025-02-15', 'Febrero', 'pendiente', 'mensual', 2025, 10, NOW()),
(3800.00, '2025-02-15', 'Febrero', 'pendiente', 'mensual', 2025, 11, NOW()),
(2500.00, '2025-02-15', 'Febrero', 'pendiente', 'mensual', 2025, 12, NOW()),
(2800.00, '2025-02-15', 'Febrero', 'pendiente', 'mensual', 2025, 13, NOW()),
(3100.00, '2025-02-15', 'Febrero', 'pendiente', 'mensual', 2025, 14, NOW()),
(6500.00, '2025-02-15', 'Febrero', 'pendiente', 'mensual', 2025, 15, NOW()),
(3000.00, '2025-02-15', 'Febrero', 'pendiente', 'mensual', 2025, 16, NOW()),
(3000.00, '2025-02-15', 'Febrero', 'pendiente', 'mensual', 2025, 17, NOW()),
(3500.00, '2025-02-15', 'Febrero', 'pendiente', 'mensual', 2025, 18, NOW());



-- Cobros VENCIDOS (Diciembre 2024)
INSERT INTO cobro (monto, fecha_vencimiento, mes_correspondiente, estado_cobro, observaciones, tipo_cobro, año_correspondiente, id_estudiante, fecha_creacion) VALUES 
(2200.00, '2024-12-15', 'Diciembre', 'vencido', 'Pendiente de contactar al estudiante', 'mensual', 2024, 11, NOW()),
(3100.00, '2024-12-15', 'Diciembre', 'vencido', 'Estudiante solicitó prórroga', 'mensual', 2024, 14, NOW());


-- 6. Insertar cobros (con los nuevos campos tipo_cobro y año_correspondiente)
INSERT INTO cobro (monto, fecha_pago, fecha_vencimiento, metodo_pago, mes_correspondiente, estado_cobro, observaciones, tipo_cobro, año_correspondiente, fecha_creacion, id_estudiante) VALUES 
-- Cobros MENSUALES pagados (Enero 2025)
(2500.00, '2025-01-05', '2025-01-10', 'efectivo', 'Enero', 'pagado', 'Pago puntual', 'mensual', 2025, NOW(), 7),
(2500.00, '2025-01-06', '2025-01-10', 'tarjeta', 'Enero', 'pagado', NULL, 'mensual', 2025, NOW(), 8),
(2800.00, '2025-01-07', '2025-01-10', 'transferencia', 'Enero', 'pagado', 'Transferencia bancaria', 'mensual', 2025, NOW(), 9),
(2800.00, '2025-01-08', '2025-01-10', 'efectivo', 'Enero', 'pagado', NULL, 'mensual', 2025, NOW(), 10),
(3000.00, '2025-01-09', '2025-01-10', 'tarjeta', 'Enero', 'pagado', NULL, 'mensual', 2025, NOW(), 11),
(3800.00, '2025-01-08', '2025-01-10', 'transferencia', 'Enero', 'pagado', 'Pago anticipado', 'mensual', 2025, NOW(), 12),

-- Cobros MENSUALES para menores (Enero 2025) - pagados por representantes
(2500.00, '2025-01-10', '2025-01-15', 'efectivo', 'Enero', 'pagado', 'Pagado por representante', 'mensual', 2025, NOW(), 13),
(2500.00, '2025-01-11', '2025-01-15', 'tarjeta', 'Enero', 'pagado', 'Pagado por representante', 'mensual', 2025, NOW(), 14),
(2500.00, '2025-01-12', '2025-01-15', 'efectivo', 'Enero', 'pagado', NULL, 'mensual', 2025, NOW(), 15),
(3500.00, '2025-01-07', '2025-01-15', 'transferencia', 'Enero', 'pagado', 'Pagado por representante', 'mensual', 2025, NOW(), 16),
(3500.00, '2025-01-08', '2025-01-15', 'efectivo', 'Enero', 'pagado', NULL, 'mensual', 2025, NOW(), 17),
(3500.00, '2025-01-09', '2025-01-15', 'tarjeta', 'Enero', 'pagado', 'Pagado por representante', 'mensual', 2025, NOW(), 18),

-- Cobros MENSUALES pendientes (Febrero 2025)
(2500.00, NULL, '2025-02-10', NULL, 'Febrero', 'pendiente', NULL, 'mensual', 2025, NOW(), 7),
(2500.00, NULL, '2025-02-10', NULL, 'Febrero', 'pendiente', NULL, 'mensual', 2025, NOW(), 8),
(2800.00, NULL, '2025-02-10', NULL, 'Febrero', 'pendiente', NULL, 'mensual', 2025, NOW(), 9),
(2800.00, NULL, '2025-02-10', NULL, 'Febrero', 'pendiente', NULL, 'mensual', 2025, NOW(), 10),
(3000.00, NULL, '2025-02-10', NULL, 'Febrero', 'pendiente', NULL, 'mensual', 2025, NOW(), 11),
(3800.00, NULL, '2025-02-10', NULL, 'Febrero', 'pendiente', NULL, 'mensual', 2025, NOW(), 12),

-- Cobros MENSUALES vencidos (ejemplo de morosidad - Diciembre 2024)
(2500.00, NULL, '2024-12-10', NULL, 'Diciembre', 'vencido', 'Pendiente de contactar', 'mensual', 2024, NOW(), 11),

(2500.00, NULL, '2025-02-10', NULL, 'Febrero', 'pendiente', NULL, 'mensual', 2025, NOW(), 7);

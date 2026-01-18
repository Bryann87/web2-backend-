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

-- Resetear secuencias (usando nombres correctos generados por EF Core con Identity columns)
ALTER SEQUENCE estilo_danza_id_estilo_seq RESTART WITH 1;
ALTER SEQUENCE persona_id_persona_seq RESTART WITH 1;
ALTER SEQUENCE clase_id_clase_seq RESTART WITH 1;
ALTER SEQUENCE inscripcion_id_insc_seq RESTART WITH 1;
ALTER SEQUENCE estado_id_estado_seq RESTART WITH 1;
ALTER SEQUENCE cobro_id_cobro_seq RESTART WITH 1;
ALTER SEQUENCE asistencia_id_asist_seq RESTART WITH 1;

-- 1. Insertar estilos de danza (ids 1-10)
INSERT INTO estilo_danza (nombre_esti, descripcion, nivel_dificultad, edad_minima, edad_maxima, activo, precio_base) VALUES 
('Salsa', 'Baile latino lleno de energia y pasion', 'Principiante', 12, NULL, true, 2500.00),
('Bachata', 'Baile romantico originario de Republica Dominicana', 'Principiante', 14, NULL, true, 2800.00),
('Merengue', 'Baile tradicional dominicano de ritmo alegre', 'Principiante', 10, NULL, true, 2300.00),
('Reggaeton', 'Baile urbano moderno con movimientos dinamicos', 'Intermedio', 13, NULL, true, 2200.00),
('Hip Hop', 'Danza urbana con estilo libre y expresivo', 'Intermedio', 8, NULL, true, 3000.00),
('Ballet', 'Danza clasica que desarrolla tecnica y elegancia', 'Principiante', 5, NULL, true, 3500.00),
('Kizomba', 'Baile sensual de origen africano', 'Intermedio', 16, NULL, true, 2900.00),
('Tango', 'Baile argentino apasionado y tecnico', 'Avanzado', 18, NULL, true, 3800.00),
('Flamenco', 'Arte espanol que combina baile, cante y guitarra', 'Intermedio', 12, NULL, true, 3300.00),
('Jazz', 'Danza moderna con influencias del jazz musical', 'Intermedio', 10, NULL, true, 3100.00);

-- 2. Insertar personas 
-- Administrador (id=1)
INSERT INTO persona (nombre, apellido, telefono, correo, rol, contrasena, activo, fecha_creacion, fecha_actualizacion) VALUES 
('Admin', 'Sistema', '809-555-0000', 'admin@academia.com', 'administrador', '$2a$12$3NPl6vAvInuxUMOIYSCaCOmFBvTILuBgXVqjc9S/HAX.fQJ4Ozpam', true, NOW(), NOW());

-- Profesores (ids 2-6)
INSERT INTO persona (nombre, apellido, telefono, correo, rol, especialidad, fecha_contrato, salario_base, contrasena, activo, fecha_creacion, fecha_actualizacion) VALUES 
('Maria', 'Zambrano', '809-555-0001', 'maria.zambrano@academia.com', 'profesor', 'Salsa y Bachata', '2023-01-15', 35000.00, '$2a$12$3NPl6vAvInuxUMOIYSCaCOmFBvTILuBgXVqjc9S/HAX.fQJ4Ozpam', true, NOW(), NOW()),
('Carlos', 'Macias', '809-555-0002', 'carlos.macias@academia.com', 'profesor', 'Hip Hop y Reggaeton', '2023-02-01', 32000.00, '$2a$12$3NPl6vAvInuxUMOIYSCaCOmFBvTILuBgXVqjc9S/HAX.fQJ4Ozpam', true, NOW(), NOW()),
('Ana', 'Cedeno', '809-555-0003', 'ana.cedeno@academia.com', 'profesor', 'Ballet Clasico', '2023-03-10', 38000.00, '$2a$12$3NPl6vAvInuxUMOIYSCaCOmFBvTILuBgXVqjc9S/HAX.fQJ4Ozpam', true, NOW(), NOW()),
('Luis', 'Moreira', '809-555-0004', 'luis.moreira@academia.com', 'profesor', 'Tango y Flamenco', '2023-04-01', 36000.00, '$2a$12$3NPl6vAvInuxUMOIYSCaCOmFBvTILuBgXVqjc9S/HAX.fQJ4Ozpam', true, NOW(), NOW()),
('Isabella', 'Vera', '809-555-0005', 'isabella.vera@academia.com', 'profesor', 'Kizomba y Jazz', '2023-05-15', 34000.00, '$2a$12$3NPl6vAvInuxUMOIYSCaCOmFBvTILuBgXVqjc9S/HAX.fQJ4Ozpam', true, NOW(), NOW());

-- Estudiantes adultos (ids 7-12)
INSERT INTO persona (nombre, apellido, telefono, correo, rol, fecha_nacimiento, genero, cedula, direccion, condiciones_medicas, activo, fecha_creacion, fecha_actualizacion) VALUES 
('Santiago', 'Quishpe', '809-555-0101', 'santiago.quishpe@email.com', 'estudiante', '1995-05-20', 'M', '001-1234567-8', 'Calle Principal #45, Quito Norte', NULL, true, NOW(), NOW()),
('Valentina', 'Arango', '809-555-0102', 'valentina.arango@email.com', 'estudiante', '2000-08-15', 'F', '001-2345678-9', 'Av. Amazonas #123', 'Alergia a ciertos perfumes', true, NOW(), NOW()),
('Sebastian', 'Cardenas', '809-555-0103', 'sebastian.cardenas@email.com', 'estudiante', '1998-12-03', 'M', '001-3456789-0', 'Calle Las Orquideas #78', NULL, true, NOW(), NOW()),
('Camila', 'Ospina', '809-555-0104', 'camila.ospina@email.com', 'estudiante', '2001-03-25', 'F', '001-4567890-1', 'Residencial El Batan', NULL, true, NOW(), NOW()),
('Andres', 'Jaramillo', '809-555-0105', 'andres.jaramillo@email.com', 'estudiante', '1997-09-10', 'M', '001-5678901-2', 'Calle El Inca #34', 'Lesion previa en rodilla izquierda', true, NOW(), NOW()),
('Mariana', 'Betancourt', '809-555-0110', 'mariana.betancourt@email.com', 'estudiante', '1994-09-22', 'F', '001-6789012-3', 'Av. 6 de Diciembre #567', NULL, true, NOW(), NOW());

-- Estudiantes menores de edad (ids 13-18)
INSERT INTO persona (nombre, apellido, telefono, correo, rol, fecha_nacimiento, genero, direccion, condiciones_medicas, activo, fecha_creacion, fecha_actualizacion) VALUES 
('Luciana', 'Chavez', '809-555-0106', 'luciana.chavez@email.com', 'estudiante', '2012-07-12', 'F', 'Calle Los Ceibos #89', NULL, true, NOW(), NOW()),
('Matias', 'Gutierrez', '809-555-0107', 'matias.gutierrez@email.com', 'estudiante', '2011-01-08', 'M', 'Urbanizacion La Carolina', NULL, true, NOW(), NOW()),
('Emiliano', 'Salazar', '809-555-0109', 'emiliano.salazar@email.com', 'estudiante', '2010-04-18', 'M', 'Calle Colon #156', NULL, true, NOW(), NOW()),
('Joaquin', 'Velasco', '809-555-0111', 'joaquin.velasco@email.com', 'estudiante', '2013-06-14', 'M', 'Av. Republica #234', 'Asma leve', true, NOW(), NOW()),
('Isabella', 'Restrepo', '809-555-0108', 'isabella.restrepo@email.com', 'estudiante', '2009-11-30', 'F', 'Calle Sucre #67', NULL, true, NOW(), NOW()),
('Antonella', 'Zuniga', '809-555-0112', 'antonella.zuniga@email.com', 'estudiante', '2014-02-28', 'F', 'Residencial Cumbaya', NULL, true, NOW(), NOW());


-- Representantes para estudiantes menores (ids 19-24)
INSERT INTO persona (nombre, apellido, telefono, correo, rol, cedula, direccion, parentesco, id_estudiante_representado, activo, fecha_creacion, fecha_actualizacion) VALUES 
('Patricia', 'Chavez', '809-555-0201', 'patricia.chavez@email.com', 'representante', '001-7890123-4', 'Calle Los Ceibos #89', 'Madre', 13, true, NOW(), NOW()),
('Ricardo', 'Gutierrez', '809-555-0202', 'ricardo.gutierrez@email.com', 'representante', '001-8901234-5', 'Urbanizacion La Carolina', 'Padre', 14, true, NOW(), NOW()),
('Adriana', 'Salazar', '809-555-0203', 'adriana.salazar@email.com', 'representante', '001-9012345-6', 'Calle Colon #156', 'Madre', 15, true, NOW(), NOW()),
('Mauricio', 'Velasco', '809-555-0204', 'mauricio.velasco@email.com', 'representante', '001-0123456-7', 'Av. Republica #234', 'Padre', 16, true, NOW(), NOW()),
('Carolina', 'Restrepo', '809-555-0205', 'carolina.restrepo@email.com', 'representante', '001-1234560-8', 'Calle Sucre #67', 'Madre', 17, true, NOW(), NOW()),
('Jorge', 'Zuniga', '809-555-0206', 'jorge.zuniga@email.com', 'representante', '001-2345601-9', 'Residencial Cumbaya', 'Padre', 18, true, NOW(), NOW());

-- 3. Insertar clases (ids 1-10)
INSERT INTO clase (nombre_clase, dia_semana, hora, duracion_minutos, capacidad_max, precio_mensu_clas, activa, fecha_inicio, id_profesor, id_estilo) VALUES 
('Salsa Principiantes', 'Lunes', '18:00:00', 60, 20, 2500.00, true, '2025-01-06', 2, 1),
('Bachata Romantica', 'Miercoles', '19:00:00', 60, 18, 2800.00, true, '2025-01-08', 2, 2),
('Hip Hop Kids', 'Martes', '16:00:00', 45, 15, 3000.00, true, '2025-01-07', 3, 5),
('Reggaeton Fitness', 'Jueves', '20:00:00', 60, 25, 2200.00, true, '2025-01-09', 3, 4),
('Ballet Infantil', 'Sabado', '09:00:00', 60, 12, 3500.00, true, '2025-01-11', 4, 6),
('Ballet Clasico Adultos', 'Sabado', '11:00:00', 90, 15, 4000.00, true, '2025-01-11', 4, 6),
('Tango Argentino', 'Viernes', '19:00:00', 75, 16, 3800.00, true, '2025-01-10', 5, 8),
('Jazz Contemporaneo', 'Miercoles', '17:00:00', 60, 18, 3100.00, true, '2025-01-08', 6, 10),
('Kizomba Sensual', 'Sabado', '18:00:00', 60, 20, 2900.00, true, '2025-01-11', 6, 7),
('Merengue Basico', 'Lunes', '17:00:00', 45, 20, 2300.00, true, '2025-01-06', 2, 3);

-- 4. Insertar inscripciones
INSERT INTO inscripcion (fecha_insc, estado, id_estudiante, id_clase) VALUES 
('2025-01-06', 'Activa', 7, 1),
('2025-01-06', 'Activa', 8, 1),
('2025-01-07', 'Activa', 9, 1),
('2025-01-08', 'Activa', 12, 1),
('2025-01-08', 'Activa', 7, 2),
('2025-01-08', 'Activa', 10, 2),
('2025-01-09', 'Activa', 12, 2),
('2025-01-07', 'Activa', 15, 3),
('2025-01-07', 'Activa', 16, 3),
('2025-01-08', 'Activa', 17, 3),
('2025-01-09', 'Activa', 8, 4),
('2025-01-09', 'Activa', 11, 4),
('2025-01-10', 'Activa', 14, 4),
('2025-01-11', 'Activa', 13, 5),
('2025-01-11', 'Activa', 18, 5),
('2025-01-11', 'Activa', 10, 6),
('2025-01-12', 'Activa', 12, 6),
('2025-01-10', 'Activa', 11, 7),
('2025-01-10', 'Activa', 9, 7),
('2025-01-08', 'Activa', 14, 8);

-- 5. Insertar estados del sistema
INSERT INTO estado (fecha_creacion, fecha_actualizacion, id_persona, no_asisto, retirado, activo) VALUES 
(NOW(), NOW(), 7, false, false, true),
(NOW(), NOW(), 8, false, false, true),
(NOW(), NOW(), 9, false, false, true),
(NOW(), NOW(), 10, false, false, true),
(NOW(), NOW(), 11, true, false, true),
(NOW(), NOW(), 12, false, false, true),
(NOW(), NOW(), 13, false, false, true),
(NOW(), NOW(), 14, false, false, true),
(NOW(), NOW(), 15, false, false, true),
(NOW(), NOW(), 16, false, false, true),
(NOW(), NOW(), 17, false, false, true),
(NOW(), NOW(), 18, false, false, true);

-- 6. Insertar cobros (Enero 2025 - pagados)
INSERT INTO cobro (monto, fecha_pago, fecha_vencimiento, metodo_pago, mes_correspondiente, estado_cobro, tipo_cobro, anio_correspondiente, id_estudiante, fecha_creacion) VALUES 
(2500.00, '2025-01-06', '2025-01-15', 'efectivo', 'Enero', 'pagado', 'mensual', 2025, 7, NOW()),
(2800.00, '2025-01-08', '2025-01-15', 'tarjeta', 'Enero', 'pagado', 'mensual', 2025, 8, NOW()),
(2500.00, '2025-01-07', '2025-01-15', 'transferencia', 'Enero', 'pagado', 'mensual', 2025, 9, NOW()),
(2800.00, '2025-01-08', '2025-01-15', 'efectivo', 'Enero', 'pagado', 'mensual', 2025, 10, NOW()),
(3000.00, '2025-01-09', '2025-01-15', 'tarjeta', 'Enero', 'pagado', 'mensual', 2025, 11, NOW()),
(3800.00, '2025-01-08', '2025-01-15', 'transferencia', 'Enero', 'pagado', 'mensual', 2025, 12, NOW()),
(3500.00, '2025-01-10', '2025-01-15', 'efectivo', 'Enero', 'pagado', 'mensual', 2025, 13, NOW()),
(2200.00, '2025-01-11', '2025-01-15', 'tarjeta', 'Enero', 'pagado', 'mensual', 2025, 14, NOW()),
(3000.00, '2025-01-07', '2025-01-15', 'efectivo', 'Enero', 'pagado', 'mensual', 2025, 15, NOW()),
(3000.00, '2025-01-07', '2025-01-15', 'tarjeta', 'Enero', 'pagado', 'mensual', 2025, 16, NOW()),
(3000.00, '2025-01-08', '2025-01-15', 'efectivo', 'Enero', 'pagado', 'mensual', 2025, 17, NOW()),
(3500.00, '2025-01-09', '2025-01-15', 'tarjeta', 'Enero', 'pagado', 'mensual', 2025, 18, NOW());

-- Cobros pendientes (Febrero 2025)
INSERT INTO cobro (monto, fecha_vencimiento, mes_correspondiente, estado_cobro, tipo_cobro, anio_correspondiente, id_estudiante, fecha_creacion) VALUES 
(2500.00, '2025-02-15', 'Febrero', 'pendiente', 'mensual', 2025, 7, NOW()),
(2800.00, '2025-02-15', 'Febrero', 'pendiente', 'mensual', 2025, 8, NOW()),
(2500.00, '2025-02-15', 'Febrero', 'pendiente', 'mensual', 2025, 9, NOW()),
(2800.00, '2025-02-15', 'Febrero', 'pendiente', 'mensual', 2025, 10, NOW()),
(3000.00, '2025-02-15', 'Febrero', 'pendiente', 'mensual', 2025, 11, NOW()),
(3800.00, '2025-02-15', 'Febrero', 'pendiente', 'mensual', 2025, 12, NOW()),
(3500.00, '2025-02-15', 'Febrero', 'pendiente', 'mensual', 2025, 13, NOW()),
(2200.00, '2025-02-15', 'Febrero', 'pendiente', 'mensual', 2025, 14, NOW()),
(3000.00, '2025-02-15', 'Febrero', 'pendiente', 'mensual', 2025, 15, NOW()),
(3000.00, '2025-02-15', 'Febrero', 'pendiente', 'mensual', 2025, 16, NOW()),
(3000.00, '2025-02-15', 'Febrero', 'pendiente', 'mensual', 2025, 17, NOW()),
(3500.00, '2025-02-15', 'Febrero', 'pendiente', 'mensual', 2025, 18, NOW());

-- Cobros vencidos (Diciembre 2024)
INSERT INTO cobro (monto, fecha_vencimiento, mes_correspondiente, estado_cobro, observaciones, tipo_cobro, anio_correspondiente, id_estudiante, fecha_creacion) VALUES 
(2200.00, '2024-12-15', 'Diciembre', 'vencido', 'Pendiente de contactar al estudiante', 'mensual', 2024, 11, NOW()),
(3100.00, '2024-12-15', 'Diciembre', 'vencido', 'Estudiante solicito prorroga', 'mensual', 2024, 14, NOW());

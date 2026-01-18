using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace academia.Migrations
{
    /// <inheritdoc />
    public partial class InitCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "audit_log",
                columns: table => new
                {
                    id_audit = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tabla_afectada = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    tipo_operacion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    id_registro = table.Column<string>(type: "text", nullable: true),
                    datos_anteriores = table.Column<string>(type: "jsonb", nullable: true),
                    datos_nuevos = table.Column<string>(type: "jsonb", nullable: true),
                    campos_modificados = table.Column<string>(type: "jsonb", nullable: true),
                    id_usuario = table.Column<int>(type: "integer", nullable: true),
                    nombre_usuario = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    rol_usuario = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ip_address = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    user_agent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    fecha_operacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    endpoint = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    metodo_http = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    duracion_ms = table.Column<long>(type: "bigint", nullable: true),
                    exitoso = table.Column<bool>(type: "boolean", nullable: false),
                    mensaje_error = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_log", x => x.id_audit);
                });

            migrationBuilder.CreateTable(
                name: "estilo_danza",
                columns: table => new
                {
                    id_estilo = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre_esti = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    descripcion = table.Column<string>(type: "text", nullable: true),
                    nivel_dificultad = table.Column<string>(type: "text", nullable: false),
                    edad_minima = table.Column<int>(type: "integer", nullable: true),
                    edad_maxima = table.Column<int>(type: "integer", nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    precio_base = table.Column<decimal>(type: "numeric", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_estilo_danza", x => x.id_estilo);
                });

            migrationBuilder.CreateTable(
                name: "persona",
                columns: table => new
                {
                    id_persona = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    apellido = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    telefono = table.Column<string>(type: "text", nullable: true),
                    correo = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    rol = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    contrasena = table.Column<string>(type: "text", nullable: true),
                    fecha_nacimiento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    genero = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    direccion = table.Column<string>(type: "text", nullable: true),
                    cedula = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    condiciones_medicas = table.Column<string>(type: "text", nullable: true),
                    especialidad = table.Column<string>(type: "text", nullable: true),
                    fecha_contrato = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    salario_base = table.Column<decimal>(type: "numeric", nullable: true),
                    parentesco = table.Column<string>(type: "text", nullable: true),
                    id_estudiante_representado = table.Column<int>(type: "integer", nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fecha_actualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_persona", x => x.id_persona);
                    table.ForeignKey(
                        name: "fk_persona_persona_id_estudiante_representado",
                        column: x => x.id_estudiante_representado,
                        principalTable: "persona",
                        principalColumn: "id_persona",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "clase",
                columns: table => new
                {
                    id_clase = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre_clase = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    dia_semana = table.Column<string>(type: "text", nullable: false),
                    hora = table.Column<TimeSpan>(type: "interval", nullable: false),
                    duracion_minutos = table.Column<int>(type: "integer", nullable: false),
                    capacidad_max = table.Column<int>(type: "integer", nullable: false),
                    precio_mensu_clas = table.Column<decimal>(type: "numeric", nullable: false),
                    activa = table.Column<bool>(type: "boolean", nullable: false),
                    fecha_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    fecha_fin = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    id_profesor = table.Column<int>(type: "integer", nullable: false),
                    id_estilo = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_clase", x => x.id_clase);
                    table.ForeignKey(
                        name: "fk_clase_estilo_danza_id_estilo",
                        column: x => x.id_estilo,
                        principalTable: "estilo_danza",
                        principalColumn: "id_estilo",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_clase_persona_id_profesor",
                        column: x => x.id_profesor,
                        principalTable: "persona",
                        principalColumn: "id_persona",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "cobro",
                columns: table => new
                {
                    id_cobro = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    monto = table.Column<decimal>(type: "numeric", nullable: false),
                    fecha_pago = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    fecha_vencimiento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    metodo_pago = table.Column<string>(type: "text", nullable: true),
                    mes_correspondiente = table.Column<string>(type: "text", nullable: false),
                    estado_cobro = table.Column<string>(type: "text", nullable: false),
                    observaciones = table.Column<string>(type: "text", nullable: true),
                    tipo_cobro = table.Column<string>(type: "text", nullable: false),
                    anio_correspondiente = table.Column<int>(type: "integer", nullable: true),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    id_estudiante = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cobro", x => x.id_cobro);
                    table.ForeignKey(
                        name: "fk_cobro_persona_id_estudiante",
                        column: x => x.id_estudiante,
                        principalTable: "persona",
                        principalColumn: "id_persona",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "estado",
                columns: table => new
                {
                    id_estado = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fecha_actualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    id_persona = table.Column<int>(type: "integer", nullable: true),
                    no_asisto = table.Column<bool>(type: "boolean", nullable: false),
                    retirado = table.Column<bool>(type: "boolean", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_estado", x => x.id_estado);
                    table.ForeignKey(
                        name: "fk_estado_persona_id_persona",
                        column: x => x.id_persona,
                        principalTable: "persona",
                        principalColumn: "id_persona",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "asistencia",
                columns: table => new
                {
                    id_asist = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fecha_asis = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    estado_asis = table.Column<string>(type: "text", nullable: false),
                    observaciones = table.Column<string>(type: "text", nullable: true),
                    registrada_por = table.Column<int>(type: "integer", nullable: true),
                    id_estudiante = table.Column<int>(type: "integer", nullable: false),
                    id_clase = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asistencia", x => x.id_asist);
                    table.ForeignKey(
                        name: "fk_asistencia_clase_id_clase",
                        column: x => x.id_clase,
                        principalTable: "clase",
                        principalColumn: "id_clase",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_asistencia_persona_id_estudiante",
                        column: x => x.id_estudiante,
                        principalTable: "persona",
                        principalColumn: "id_persona",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_asistencia_persona_registrada_por",
                        column: x => x.registrada_por,
                        principalTable: "persona",
                        principalColumn: "id_persona");
                });

            migrationBuilder.CreateTable(
                name: "inscripcion",
                columns: table => new
                {
                    id_insc = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fecha_insc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    estado = table.Column<string>(type: "text", nullable: false),
                    fecha_baja = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    motivo_baja = table.Column<string>(type: "text", nullable: true),
                    id_estudiante = table.Column<int>(type: "integer", nullable: false),
                    id_clase = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inscripcion", x => x.id_insc);
                    table.ForeignKey(
                        name: "fk_inscripcion_clase_id_clase",
                        column: x => x.id_clase,
                        principalTable: "clase",
                        principalColumn: "id_clase",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_inscripcion_persona_id_estudiante",
                        column: x => x.id_estudiante,
                        principalTable: "persona",
                        principalColumn: "id_persona",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_asistencia_id_clase",
                table: "asistencia",
                column: "id_clase");

            migrationBuilder.CreateIndex(
                name: "ix_asistencia_id_estudiante",
                table: "asistencia",
                column: "id_estudiante");

            migrationBuilder.CreateIndex(
                name: "ix_asistencia_registrada_por",
                table: "asistencia",
                column: "registrada_por");

            migrationBuilder.CreateIndex(
                name: "ix_audit_log_fecha_operacion",
                table: "audit_log",
                column: "fecha_operacion");

            migrationBuilder.CreateIndex(
                name: "ix_audit_log_id_usuario",
                table: "audit_log",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "ix_audit_log_tabla_afectada",
                table: "audit_log",
                column: "tabla_afectada");

            migrationBuilder.CreateIndex(
                name: "ix_audit_log_tabla_afectada_id_registro",
                table: "audit_log",
                columns: new[] { "tabla_afectada", "id_registro" });

            migrationBuilder.CreateIndex(
                name: "ix_clase_id_estilo",
                table: "clase",
                column: "id_estilo");

            migrationBuilder.CreateIndex(
                name: "ix_clase_id_profesor",
                table: "clase",
                column: "id_profesor");

            migrationBuilder.CreateIndex(
                name: "ix_cobro_id_estudiante",
                table: "cobro",
                column: "id_estudiante");

            migrationBuilder.CreateIndex(
                name: "ix_estado_id_persona",
                table: "estado",
                column: "id_persona");

            migrationBuilder.CreateIndex(
                name: "ix_inscripcion_id_clase",
                table: "inscripcion",
                column: "id_clase");

            migrationBuilder.CreateIndex(
                name: "ix_inscripcion_id_estudiante",
                table: "inscripcion",
                column: "id_estudiante");

            migrationBuilder.CreateIndex(
                name: "ix_persona_correo",
                table: "persona",
                column: "correo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_persona_id_estudiante_representado",
                table: "persona",
                column: "id_estudiante_representado");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "asistencia");

            migrationBuilder.DropTable(
                name: "audit_log");

            migrationBuilder.DropTable(
                name: "cobro");

            migrationBuilder.DropTable(
                name: "estado");

            migrationBuilder.DropTable(
                name: "inscripcion");

            migrationBuilder.DropTable(
                name: "clase");

            migrationBuilder.DropTable(
                name: "estilo_danza");

            migrationBuilder.DropTable(
                name: "persona");
        }
    }
}

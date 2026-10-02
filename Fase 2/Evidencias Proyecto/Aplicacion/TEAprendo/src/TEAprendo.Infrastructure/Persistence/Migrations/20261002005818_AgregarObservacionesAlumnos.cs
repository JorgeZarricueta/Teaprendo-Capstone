using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TEAprendo.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarObservacionesAlumnos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "observaciones_alumnos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    alumno_id = table.Column<Guid>(type: "uuid", nullable: false),
                    autor_usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    actividad_id = table.Column<Guid>(type: "uuid", nullable: true),
                    fecha_observacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    contexto = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    categoria = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    situacion = table.Column<string>(type: "character varying(1500)", maxLength: 1500, nullable: false),
                    desencadenante = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    apoyo_aplicado = table.Column<string>(type: "character varying(1500)", maxLength: 1500, nullable: true),
                    respuesta_alumno = table.Column<string>(type: "character varying(1500)", maxLength: 1500, nullable: false),
                    se_estabilizo = table.Column<bool>(type: "boolean", nullable: true),
                    seguimiento = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    activa = table.Column<bool>(type: "boolean", nullable: false),
                    fecha_creacion_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fecha_actualizacion_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_observaciones_alumnos", x => x.id);
                    table.ForeignKey(
                        name: "FK_observaciones_alumnos_actividades_actividad_id",
                        column: x => x.actividad_id,
                        principalTable: "actividades",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_observaciones_alumnos_alumnos_alumno_id",
                        column: x => x.alumno_id,
                        principalTable: "alumnos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_observaciones_alumnos_usuarios_autor_usuario_id",
                        column: x => x.autor_usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_observaciones_alumnos_actividad_id",
                table: "observaciones_alumnos",
                column: "actividad_id");

            migrationBuilder.CreateIndex(
                name: "IX_observaciones_alumnos_alumno_id_fecha_observacion",
                table: "observaciones_alumnos",
                columns: new[] { "alumno_id", "fecha_observacion" });

            migrationBuilder.CreateIndex(
                name: "IX_observaciones_alumnos_autor_usuario_id",
                table: "observaciones_alumnos",
                column: "autor_usuario_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "observaciones_alumnos");
        }
    }
}

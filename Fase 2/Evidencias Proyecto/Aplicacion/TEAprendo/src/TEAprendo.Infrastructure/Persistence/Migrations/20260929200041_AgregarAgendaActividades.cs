using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TEAprendo.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarAgendaActividades : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "actividades",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    alumno_id = table.Column<Guid>(type: "uuid", nullable: false),
                    creado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    titulo = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    tipo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    fecha_hora_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fecha_hora_fin = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    activa = table.Column<bool>(type: "boolean", nullable: false),
                    fecha_creacion_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_actividades", x => x.id);
                    table.ForeignKey(
                        name: "FK_actividades_alumnos_alumno_id",
                        column: x => x.alumno_id,
                        principalTable: "alumnos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_actividades_alumno_id",
                table: "actividades",
                column: "alumno_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "actividades");
        }
    }
}

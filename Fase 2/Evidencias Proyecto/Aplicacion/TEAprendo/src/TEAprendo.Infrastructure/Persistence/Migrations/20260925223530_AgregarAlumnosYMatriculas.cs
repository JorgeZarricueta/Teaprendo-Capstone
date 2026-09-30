using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TEAprendo.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarAlumnosYMatriculas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "alumnos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    apellido = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    fecha_nacimiento = table.Column<DateTime>(type: "date", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    fecha_creacion_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_alumnos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "matriculas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    alumno_id = table.Column<Guid>(type: "uuid", nullable: false),
                    aula_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fecha_inicio = table.Column<DateTime>(type: "date", nullable: false),
                    fecha_fin = table.Column<DateTime>(type: "date", nullable: true),
                    activa = table.Column<bool>(type: "boolean", nullable: false),
                    fecha_creacion_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_matriculas", x => x.id);
                    table.ForeignKey(
                        name: "FK_matriculas_alumnos_alumno_id",
                        column: x => x.alumno_id,
                        principalTable: "alumnos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_matriculas_aulas_aula_id",
                        column: x => x.aula_id,
                        principalTable: "aulas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_matriculas_alumno_id",
                table: "matriculas",
                column: "alumno_id");

            migrationBuilder.CreateIndex(
                name: "IX_matriculas_aula_id",
                table: "matriculas",
                column: "aula_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "matriculas");

            migrationBuilder.DropTable(
                name: "alumnos");
        }
    }
}

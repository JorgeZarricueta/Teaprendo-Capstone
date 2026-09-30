using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TEAprendo.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarApoderadosAlumnos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "apoderados_alumnos",
                columns: table => new
                {
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    alumno_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_apoderados_alumnos", x => new { x.usuario_id, x.alumno_id });
                    table.ForeignKey(
                        name: "FK_apoderados_alumnos_alumnos_alumno_id",
                        column: x => x.alumno_id,
                        principalTable: "alumnos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_apoderados_alumnos_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_apoderados_alumnos_alumno_id",
                table: "apoderados_alumnos",
                column: "alumno_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "apoderados_alumnos");
        }
    }
}

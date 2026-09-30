using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TEAprendo.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarDocentesAulas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "docentes_aulas",
                columns: table => new
                {
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    aula_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_docentes_aulas", x => new { x.usuario_id, x.aula_id });
                    table.ForeignKey(
                        name: "FK_docentes_aulas_aulas_aula_id",
                        column: x => x.aula_id,
                        principalTable: "aulas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_docentes_aulas_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_docentes_aulas_aula_id",
                table: "docentes_aulas",
                column: "aula_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "docentes_aulas");
        }
    }
}

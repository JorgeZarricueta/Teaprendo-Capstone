using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TEAprendo.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class TraducirColumnasEstructuraInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_aulas_sedes_SiteId",
                table: "aulas");

            migrationBuilder.DropForeignKey(
                name: "FK_sedes_instituciones_InstitutionId",
                table: "sedes");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "sedes",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "sedes",
                newName: "nombre");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "sedes",
                newName: "activa");

            migrationBuilder.RenameColumn(
                name: "InstitutionId",
                table: "sedes",
                newName: "institucion_id");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                table: "sedes",
                newName: "fecha_creacion_utc");

            migrationBuilder.RenameColumn(
                name: "Address",
                table: "sedes",
                newName: "direccion");

            migrationBuilder.RenameIndex(
                name: "IX_sedes_InstitutionId",
                table: "sedes",
                newName: "IX_sedes_institucion_id");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "instituciones",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "instituciones",
                newName: "nombre");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "instituciones",
                newName: "activa");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                table: "instituciones",
                newName: "fecha_creacion_utc");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "aulas",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "SiteId",
                table: "aulas",
                newName: "sede_id");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "aulas",
                newName: "nombre");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "aulas",
                newName: "activa");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                table: "aulas",
                newName: "fecha_creacion_utc");

            migrationBuilder.RenameColumn(
                name: "AcademicYear",
                table: "aulas",
                newName: "anio_academico");

            migrationBuilder.RenameIndex(
                name: "IX_aulas_SiteId",
                table: "aulas",
                newName: "IX_aulas_sede_id");

            migrationBuilder.AddForeignKey(
                name: "FK_aulas_sedes_sede_id",
                table: "aulas",
                column: "sede_id",
                principalTable: "sedes",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_sedes_instituciones_institucion_id",
                table: "sedes",
                column: "institucion_id",
                principalTable: "instituciones",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_aulas_sedes_sede_id",
                table: "aulas");

            migrationBuilder.DropForeignKey(
                name: "FK_sedes_instituciones_institucion_id",
                table: "sedes");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "sedes",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "nombre",
                table: "sedes",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "institucion_id",
                table: "sedes",
                newName: "InstitutionId");

            migrationBuilder.RenameColumn(
                name: "fecha_creacion_utc",
                table: "sedes",
                newName: "CreatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "direccion",
                table: "sedes",
                newName: "Address");

            migrationBuilder.RenameColumn(
                name: "activa",
                table: "sedes",
                newName: "IsActive");

            migrationBuilder.RenameIndex(
                name: "IX_sedes_institucion_id",
                table: "sedes",
                newName: "IX_sedes_InstitutionId");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "instituciones",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "nombre",
                table: "instituciones",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "fecha_creacion_utc",
                table: "instituciones",
                newName: "CreatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "activa",
                table: "instituciones",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "aulas",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "sede_id",
                table: "aulas",
                newName: "SiteId");

            migrationBuilder.RenameColumn(
                name: "nombre",
                table: "aulas",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "fecha_creacion_utc",
                table: "aulas",
                newName: "CreatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "anio_academico",
                table: "aulas",
                newName: "AcademicYear");

            migrationBuilder.RenameColumn(
                name: "activa",
                table: "aulas",
                newName: "IsActive");

            migrationBuilder.RenameIndex(
                name: "IX_aulas_sede_id",
                table: "aulas",
                newName: "IX_aulas_SiteId");

            migrationBuilder.AddForeignKey(
                name: "FK_aulas_sedes_SiteId",
                table: "aulas",
                column: "SiteId",
                principalTable: "sedes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_sedes_instituciones_InstitutionId",
                table: "sedes",
                column: "InstitutionId",
                principalTable: "instituciones",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

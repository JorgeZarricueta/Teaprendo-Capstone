using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TEAprendo.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarEventosCrisis1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CrisisEvent_alumnos_StudentId",
                table: "CrisisEvent");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CrisisEvent",
                table: "CrisisEvent");

            migrationBuilder.RenameTable(
                name: "CrisisEvent",
                newName: "eventos_crisis");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "eventos_crisis",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "WasStabilized",
                table: "eventos_crisis",
                newName: "se_estabilizo");

            migrationBuilder.RenameColumn(
                name: "Trigger",
                table: "eventos_crisis",
                newName: "desencadenante");

            migrationBuilder.RenameColumn(
                name: "StudentId",
                table: "eventos_crisis",
                newName: "alumno_id");

            migrationBuilder.RenameColumn(
                name: "RequiredAdditionalSupport",
                table: "eventos_crisis",
                newName: "requirio_apoyo_adicional");

            migrationBuilder.RenameColumn(
                name: "Outcome",
                table: "eventos_crisis",
                newName: "resultado");

            migrationBuilder.RenameColumn(
                name: "Notes",
                table: "eventos_crisis",
                newName: "notas");

            migrationBuilder.RenameColumn(
                name: "ImmediateSupport",
                table: "eventos_crisis",
                newName: "apoyo_inmediato");

            migrationBuilder.RenameColumn(
                name: "EventDescription",
                table: "eventos_crisis",
                newName: "descripcion_evento");

            migrationBuilder.RenameColumn(
                name: "EventDateTime",
                table: "eventos_crisis",
                newName: "fecha_hora_evento");

            migrationBuilder.RenameColumn(
                name: "DurationMinutes",
                table: "eventos_crisis",
                newName: "duracion_minutos");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "eventos_crisis",
                newName: "creado_por_usuario_id");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                table: "eventos_crisis",
                newName: "fecha_creacion_utc");

            migrationBuilder.RenameColumn(
                name: "Context",
                table: "eventos_crisis",
                newName: "contexto");

            migrationBuilder.RenameIndex(
                name: "IX_CrisisEvent_StudentId",
                table: "eventos_crisis",
                newName: "IX_eventos_crisis_alumno_id");

            migrationBuilder.AlterColumn<string>(
                name: "desencadenante",
                table: "eventos_crisis",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "requirio_apoyo_adicional",
                table: "eventos_crisis",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<string>(
                name: "resultado",
                table: "eventos_crisis",
                type: "character varying(1500)",
                maxLength: 1500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "notas",
                table: "eventos_crisis",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "apoyo_inmediato",
                table: "eventos_crisis",
                type: "character varying(1500)",
                maxLength: 1500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "descripcion_evento",
                table: "eventos_crisis",
                type: "character varying(1500)",
                maxLength: 1500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<DateTime>(
                name: "fecha_hora_evento",
                table: "eventos_crisis",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "contexto",
                table: "eventos_crisis",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddPrimaryKey(
                name: "PK_eventos_crisis",
                table: "eventos_crisis",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_eventos_crisis_alumnos_alumno_id",
                table: "eventos_crisis",
                column: "alumno_id",
                principalTable: "alumnos",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_eventos_crisis_alumnos_alumno_id",
                table: "eventos_crisis");

            migrationBuilder.DropPrimaryKey(
                name: "PK_eventos_crisis",
                table: "eventos_crisis");

            migrationBuilder.RenameTable(
                name: "eventos_crisis",
                newName: "CrisisEvent");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "CrisisEvent",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "se_estabilizo",
                table: "CrisisEvent",
                newName: "WasStabilized");

            migrationBuilder.RenameColumn(
                name: "resultado",
                table: "CrisisEvent",
                newName: "Outcome");

            migrationBuilder.RenameColumn(
                name: "requirio_apoyo_adicional",
                table: "CrisisEvent",
                newName: "RequiredAdditionalSupport");

            migrationBuilder.RenameColumn(
                name: "notas",
                table: "CrisisEvent",
                newName: "Notes");

            migrationBuilder.RenameColumn(
                name: "fecha_hora_evento",
                table: "CrisisEvent",
                newName: "EventDateTime");

            migrationBuilder.RenameColumn(
                name: "fecha_creacion_utc",
                table: "CrisisEvent",
                newName: "CreatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "duracion_minutos",
                table: "CrisisEvent",
                newName: "DurationMinutes");

            migrationBuilder.RenameColumn(
                name: "desencadenante",
                table: "CrisisEvent",
                newName: "Trigger");

            migrationBuilder.RenameColumn(
                name: "descripcion_evento",
                table: "CrisisEvent",
                newName: "EventDescription");

            migrationBuilder.RenameColumn(
                name: "creado_por_usuario_id",
                table: "CrisisEvent",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "contexto",
                table: "CrisisEvent",
                newName: "Context");

            migrationBuilder.RenameColumn(
                name: "apoyo_inmediato",
                table: "CrisisEvent",
                newName: "ImmediateSupport");

            migrationBuilder.RenameColumn(
                name: "alumno_id",
                table: "CrisisEvent",
                newName: "StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_eventos_crisis_alumno_id",
                table: "CrisisEvent",
                newName: "IX_CrisisEvent_StudentId");

            migrationBuilder.AlterColumn<string>(
                name: "Outcome",
                table: "CrisisEvent",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1500)",
                oldMaxLength: 1500,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "RequiredAdditionalSupport",
                table: "CrisisEvent",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "CrisisEvent",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "EventDateTime",
                table: "CrisisEvent",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<string>(
                name: "Trigger",
                table: "CrisisEvent",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EventDescription",
                table: "CrisisEvent",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(1500)",
                oldMaxLength: 1500);

            migrationBuilder.AlterColumn<string>(
                name: "Context",
                table: "CrisisEvent",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "ImmediateSupport",
                table: "CrisisEvent",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1500)",
                oldMaxLength: 1500,
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_CrisisEvent",
                table: "CrisisEvent",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CrisisEvent_alumnos_StudentId",
                table: "CrisisEvent",
                column: "StudentId",
                principalTable: "alumnos",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

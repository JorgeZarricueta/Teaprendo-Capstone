using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TEAprendo.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarModeloSeguimiento2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BehavioralObservation_alumnos_StudentId",
                table: "BehavioralObservation");

            migrationBuilder.DropForeignKey(
                name: "FK_Intervention_BehavioralObservation_BehavioralObservationId",
                table: "Intervention");

            migrationBuilder.DropForeignKey(
                name: "FK_InterventionResult_Intervention_InterventionId",
                table: "InterventionResult");

            migrationBuilder.DropPrimaryKey(
                name: "PK_InterventionResult",
                table: "InterventionResult");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Intervention",
                table: "Intervention");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BehavioralObservation",
                table: "BehavioralObservation");

            migrationBuilder.RenameTable(
                name: "InterventionResult",
                newName: "resultados_intervencion");

            migrationBuilder.RenameTable(
                name: "Intervention",
                newName: "intervenciones");

            migrationBuilder.RenameTable(
                name: "BehavioralObservation",
                newName: "observaciones_conductuales");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "resultados_intervencion",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "ResultDateTime",
                table: "resultados_intervencion",
                newName: "fecha_hora_resultado");

            migrationBuilder.RenameColumn(
                name: "Outcome",
                table: "resultados_intervencion",
                newName: "resultado");

            migrationBuilder.RenameColumn(
                name: "Notes",
                table: "resultados_intervencion",
                newName: "notas");

            migrationBuilder.RenameColumn(
                name: "InterventionId",
                table: "resultados_intervencion",
                newName: "intervencion_id");

            migrationBuilder.RenameColumn(
                name: "Effectiveness",
                table: "resultados_intervencion",
                newName: "efectividad");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "resultados_intervencion",
                newName: "creado_por_usuario_id");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                table: "resultados_intervencion",
                newName: "fecha_creacion_utc");

            migrationBuilder.RenameIndex(
                name: "IX_InterventionResult_InterventionId",
                table: "resultados_intervencion",
                newName: "IX_resultados_intervencion_intervencion_id");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "intervenciones",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "Strategy",
                table: "intervenciones",
                newName: "estrategia");

            migrationBuilder.RenameColumn(
                name: "Objective",
                table: "intervenciones",
                newName: "objetivo");

            migrationBuilder.RenameColumn(
                name: "InterventionDateTime",
                table: "intervenciones",
                newName: "fecha_hora_intervencion");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "intervenciones",
                newName: "creado_por_usuario_id");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                table: "intervenciones",
                newName: "fecha_creacion_utc");

            migrationBuilder.RenameColumn(
                name: "BehavioralObservationId",
                table: "intervenciones",
                newName: "observacion_id");

            migrationBuilder.RenameIndex(
                name: "IX_Intervention_BehavioralObservationId",
                table: "intervenciones",
                newName: "IX_intervenciones_observacion_id");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "observaciones_conductuales",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "StudentId",
                table: "observaciones_conductuales",
                newName: "alumno_id");

            migrationBuilder.RenameColumn(
                name: "ObservationDateTime",
                table: "observaciones_conductuales",
                newName: "fecha_hora_observacion");

            migrationBuilder.RenameColumn(
                name: "Notes",
                table: "observaciones_conductuales",
                newName: "notas");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "observaciones_conductuales",
                newName: "creado_por_usuario_id");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                table: "observaciones_conductuales",
                newName: "fecha_creacion_utc");

            migrationBuilder.RenameColumn(
                name: "Context",
                table: "observaciones_conductuales",
                newName: "contexto");

            migrationBuilder.RenameColumn(
                name: "Consequence",
                table: "observaciones_conductuales",
                newName: "consecuencia");

            migrationBuilder.RenameColumn(
                name: "Behavior",
                table: "observaciones_conductuales",
                newName: "conducta");

            migrationBuilder.RenameColumn(
                name: "Antecedent",
                table: "observaciones_conductuales",
                newName: "antecedente");

            migrationBuilder.RenameIndex(
                name: "IX_BehavioralObservation_StudentId",
                table: "observaciones_conductuales",
                newName: "IX_observaciones_conductuales_alumno_id");

            migrationBuilder.AlterColumn<DateTime>(
                name: "fecha_hora_resultado",
                table: "resultados_intervencion",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "resultado",
                table: "resultados_intervencion",
                type: "character varying(1500)",
                maxLength: 1500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "notas",
                table: "resultados_intervencion",
                type: "character varying(1500)",
                maxLength: 1500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "efectividad",
                table: "resultados_intervencion",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "estrategia",
                table: "intervenciones",
                type: "character varying(1500)",
                maxLength: 1500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "objetivo",
                table: "intervenciones",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "fecha_hora_intervencion",
                table: "intervenciones",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "fecha_hora_observacion",
                table: "observaciones_conductuales",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "notas",
                table: "observaciones_conductuales",
                type: "character varying(1500)",
                maxLength: 1500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "contexto",
                table: "observaciones_conductuales",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "consecuencia",
                table: "observaciones_conductuales",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "conducta",
                table: "observaciones_conductuales",
                type: "character varying(1500)",
                maxLength: 1500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "antecedente",
                table: "observaciones_conductuales",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_resultados_intervencion",
                table: "resultados_intervencion",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_intervenciones",
                table: "intervenciones",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_observaciones_conductuales",
                table: "observaciones_conductuales",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "IX_resultados_intervencion_creado_por_usuario_id",
                table: "resultados_intervencion",
                column: "creado_por_usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_intervenciones_creado_por_usuario_id",
                table: "intervenciones",
                column: "creado_por_usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_observaciones_conductuales_creado_por_usuario_id",
                table: "observaciones_conductuales",
                column: "creado_por_usuario_id");

            migrationBuilder.AddForeignKey(
                name: "FK_intervenciones_observaciones_conductuales_observacion_id",
                table: "intervenciones",
                column: "observacion_id",
                principalTable: "observaciones_conductuales",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_intervenciones_usuarios_creado_por_usuario_id",
                table: "intervenciones",
                column: "creado_por_usuario_id",
                principalTable: "usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_observaciones_conductuales_alumnos_alumno_id",
                table: "observaciones_conductuales",
                column: "alumno_id",
                principalTable: "alumnos",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_observaciones_conductuales_usuarios_creado_por_usuario_id",
                table: "observaciones_conductuales",
                column: "creado_por_usuario_id",
                principalTable: "usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_resultados_intervencion_intervenciones_intervencion_id",
                table: "resultados_intervencion",
                column: "intervencion_id",
                principalTable: "intervenciones",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_resultados_intervencion_usuarios_creado_por_usuario_id",
                table: "resultados_intervencion",
                column: "creado_por_usuario_id",
                principalTable: "usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_intervenciones_observaciones_conductuales_observacion_id",
                table: "intervenciones");

            migrationBuilder.DropForeignKey(
                name: "FK_intervenciones_usuarios_creado_por_usuario_id",
                table: "intervenciones");

            migrationBuilder.DropForeignKey(
                name: "FK_observaciones_conductuales_alumnos_alumno_id",
                table: "observaciones_conductuales");

            migrationBuilder.DropForeignKey(
                name: "FK_observaciones_conductuales_usuarios_creado_por_usuario_id",
                table: "observaciones_conductuales");

            migrationBuilder.DropForeignKey(
                name: "FK_resultados_intervencion_intervenciones_intervencion_id",
                table: "resultados_intervencion");

            migrationBuilder.DropForeignKey(
                name: "FK_resultados_intervencion_usuarios_creado_por_usuario_id",
                table: "resultados_intervencion");

            migrationBuilder.DropPrimaryKey(
                name: "PK_resultados_intervencion",
                table: "resultados_intervencion");

            migrationBuilder.DropIndex(
                name: "IX_resultados_intervencion_creado_por_usuario_id",
                table: "resultados_intervencion");

            migrationBuilder.DropPrimaryKey(
                name: "PK_observaciones_conductuales",
                table: "observaciones_conductuales");

            migrationBuilder.DropIndex(
                name: "IX_observaciones_conductuales_creado_por_usuario_id",
                table: "observaciones_conductuales");

            migrationBuilder.DropPrimaryKey(
                name: "PK_intervenciones",
                table: "intervenciones");

            migrationBuilder.DropIndex(
                name: "IX_intervenciones_creado_por_usuario_id",
                table: "intervenciones");

            migrationBuilder.RenameTable(
                name: "resultados_intervencion",
                newName: "InterventionResult");

            migrationBuilder.RenameTable(
                name: "observaciones_conductuales",
                newName: "BehavioralObservation");

            migrationBuilder.RenameTable(
                name: "intervenciones",
                newName: "Intervention");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "InterventionResult",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "resultado",
                table: "InterventionResult",
                newName: "Outcome");

            migrationBuilder.RenameColumn(
                name: "notas",
                table: "InterventionResult",
                newName: "Notes");

            migrationBuilder.RenameColumn(
                name: "intervencion_id",
                table: "InterventionResult",
                newName: "InterventionId");

            migrationBuilder.RenameColumn(
                name: "fecha_hora_resultado",
                table: "InterventionResult",
                newName: "ResultDateTime");

            migrationBuilder.RenameColumn(
                name: "fecha_creacion_utc",
                table: "InterventionResult",
                newName: "CreatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "efectividad",
                table: "InterventionResult",
                newName: "Effectiveness");

            migrationBuilder.RenameColumn(
                name: "creado_por_usuario_id",
                table: "InterventionResult",
                newName: "CreatedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_resultados_intervencion_intervencion_id",
                table: "InterventionResult",
                newName: "IX_InterventionResult_InterventionId");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "BehavioralObservation",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "notas",
                table: "BehavioralObservation",
                newName: "Notes");

            migrationBuilder.RenameColumn(
                name: "fecha_hora_observacion",
                table: "BehavioralObservation",
                newName: "ObservationDateTime");

            migrationBuilder.RenameColumn(
                name: "fecha_creacion_utc",
                table: "BehavioralObservation",
                newName: "CreatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "creado_por_usuario_id",
                table: "BehavioralObservation",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "contexto",
                table: "BehavioralObservation",
                newName: "Context");

            migrationBuilder.RenameColumn(
                name: "consecuencia",
                table: "BehavioralObservation",
                newName: "Consequence");

            migrationBuilder.RenameColumn(
                name: "conducta",
                table: "BehavioralObservation",
                newName: "Behavior");

            migrationBuilder.RenameColumn(
                name: "antecedente",
                table: "BehavioralObservation",
                newName: "Antecedent");

            migrationBuilder.RenameColumn(
                name: "alumno_id",
                table: "BehavioralObservation",
                newName: "StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_observaciones_conductuales_alumno_id",
                table: "BehavioralObservation",
                newName: "IX_BehavioralObservation_StudentId");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Intervention",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "observacion_id",
                table: "Intervention",
                newName: "BehavioralObservationId");

            migrationBuilder.RenameColumn(
                name: "objetivo",
                table: "Intervention",
                newName: "Objective");

            migrationBuilder.RenameColumn(
                name: "fecha_hora_intervencion",
                table: "Intervention",
                newName: "InterventionDateTime");

            migrationBuilder.RenameColumn(
                name: "fecha_creacion_utc",
                table: "Intervention",
                newName: "CreatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "estrategia",
                table: "Intervention",
                newName: "Strategy");

            migrationBuilder.RenameColumn(
                name: "creado_por_usuario_id",
                table: "Intervention",
                newName: "CreatedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_intervenciones_observacion_id",
                table: "Intervention",
                newName: "IX_Intervention_BehavioralObservationId");

            migrationBuilder.AlterColumn<string>(
                name: "Outcome",
                table: "InterventionResult",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(1500)",
                oldMaxLength: 1500);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "InterventionResult",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1500)",
                oldMaxLength: 1500,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ResultDateTime",
                table: "InterventionResult",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<string>(
                name: "Effectiveness",
                table: "InterventionResult",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "BehavioralObservation",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1500)",
                oldMaxLength: 1500,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ObservationDateTime",
                table: "BehavioralObservation",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<string>(
                name: "Context",
                table: "BehavioralObservation",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<string>(
                name: "Consequence",
                table: "BehavioralObservation",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Behavior",
                table: "BehavioralObservation",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(1500)",
                oldMaxLength: 1500);

            migrationBuilder.AlterColumn<string>(
                name: "Antecedent",
                table: "BehavioralObservation",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Objective",
                table: "Intervention",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "InterventionDateTime",
                table: "Intervention",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<string>(
                name: "Strategy",
                table: "Intervention",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(1500)",
                oldMaxLength: 1500);

            migrationBuilder.AddPrimaryKey(
                name: "PK_InterventionResult",
                table: "InterventionResult",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BehavioralObservation",
                table: "BehavioralObservation",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Intervention",
                table: "Intervention",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BehavioralObservation_alumnos_StudentId",
                table: "BehavioralObservation",
                column: "StudentId",
                principalTable: "alumnos",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Intervention_BehavioralObservation_BehavioralObservationId",
                table: "Intervention",
                column: "BehavioralObservationId",
                principalTable: "BehavioralObservation",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InterventionResult_Intervention_InterventionId",
                table: "InterventionResult",
                column: "InterventionId",
                principalTable: "Intervention",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

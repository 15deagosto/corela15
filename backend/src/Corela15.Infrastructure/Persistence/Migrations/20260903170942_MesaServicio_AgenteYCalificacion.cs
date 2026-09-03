using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MesaServicio_AgenteYCalificacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "calificacion",
                schema: "mesaservicio",
                table: "ticket",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "comentario_calificacion",
                schema: "mesaservicio",
                table: "ticket",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "fecha_calificacion",
                schema: "mesaservicio",
                table: "ticket",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "fecha_primera_respuesta",
                schema: "mesaservicio",
                table: "ticket",
                type: "timestamp with time zone",
                nullable: true);

            // Segundo menú real de Mesa de Servicio — a diferencia de
            // 'mesa-servicio' (universal, inyectado siempre por AuthService),
            // este SÍ pasa por rol_menu normal: quien lo tenga puede tomar/
            // reasignar/cambiar estado de un ticket. Se otorga a ADMINISTRADOR
            // por defecto (mismo patrón que cada menú nuevo del proyecto) —
            // el resto de agentes reales se agregan desde Configuración →
            // Roles → Permisos, sin tocar código.
            migrationBuilder.Sql("""
                INSERT INTO seguridad.menu (codigo, nombre, orden, activo) VALUES
                    ('mesa-servicio-agente', 'Mesa de Servicio — Agente', 18, true);

                INSERT INTO seguridad.rol_menu (id_rol, id_menu, activo)
                SELECT r.id, m.id, true
                FROM seguridad.rol r, seguridad.menu m
                WHERE r.nombre = 'ADMINISTRADOR' AND m.codigo = 'mesa-servicio-agente';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM seguridad.rol_menu WHERE id_menu = (SELECT id FROM seguridad.menu WHERE codigo = 'mesa-servicio-agente');
                DELETE FROM seguridad.menu WHERE codigo = 'mesa-servicio-agente';
                """);

            migrationBuilder.DropColumn(
                name: "calificacion",
                schema: "mesaservicio",
                table: "ticket");

            migrationBuilder.DropColumn(
                name: "comentario_calificacion",
                schema: "mesaservicio",
                table: "ticket");

            migrationBuilder.DropColumn(
                name: "fecha_calificacion",
                schema: "mesaservicio",
                table: "ticket");

            migrationBuilder.DropColumn(
                name: "fecha_primera_respuesta",
                schema: "mesaservicio",
                table: "ticket");
        }
    }
}

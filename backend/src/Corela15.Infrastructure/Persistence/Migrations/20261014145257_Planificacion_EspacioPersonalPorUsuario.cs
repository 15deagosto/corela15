using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Planificacion_EspacioPersonalPorUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "id_usuario",
                schema: "planificacion",
                table: "area",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_area_id_usuario",
                schema: "planificacion",
                table: "area",
                column: "id_usuario",
                unique: true,
                filter: "id_usuario IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "fk_area_usuarios_id_usuario",
                schema: "planificacion",
                table: "area",
                column: "id_usuario",
                principalSchema: "seguridad",
                principalTable: "usuario",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            // La planificación pasa a ser personal: las áreas de
            // departamento previas quedan inactivas (solo historia de los
            // planes ya registrados con ellas, nadie puede escribir ahí) y
            // se desvincula a los usuarios -- cada uno recibe su espacio
            // personal solo la próxima vez que abra el módulo. Nunca se
            // borra nada (mismo criterio de todo el core).
            migrationBuilder.Sql("UPDATE planificacion.area SET activo = false WHERE id_usuario IS NULL;");
            migrationBuilder.Sql("UPDATE seguridad.usuario SET codigo_area_planificacion = NULL WHERE codigo_area_planificacion IS NOT NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_area_usuarios_id_usuario",
                schema: "planificacion",
                table: "area");

            migrationBuilder.DropIndex(
                name: "ix_area_id_usuario",
                schema: "planificacion",
                table: "area");

            migrationBuilder.DropColumn(
                name: "id_usuario",
                schema: "planificacion",
                table: "area");
        }
    }
}

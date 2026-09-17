using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Comunicacion_PropietarioYPrivacidad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "es_publico",
                schema: "comunicacion",
                table: "canal",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<Guid>(
                name: "id_propietario",
                schema: "comunicacion",
                table: "canal",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_canal_id_propietario",
                schema: "comunicacion",
                table: "canal",
                column: "id_propietario");

            migrationBuilder.AddForeignKey(
                name: "fk_canal_usuarios_id_propietario",
                schema: "comunicacion",
                table: "canal",
                column: "id_propietario",
                principalSchema: "seguridad",
                principalTable: "usuario",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            // Backfill real: dueño = quien creó el canal, tomado de la
            // columna vieja `creado_por` (guardaba el Guid como texto) --
            // pero los canales sembrados por migración ("Cajas"/"Balcón de
            // Servicio") tienen ahí el literal 'seed:comunicacion_interna',
            // no un Guid real, así que quedan sin dueño (NULL) hasta que un
            // administrador les asigne uno real -- nunca se inventa un
            // dueño para esos.
            migrationBuilder.Sql(@"
                UPDATE comunicacion.canal
                SET id_propietario = creado_por::uuid
                WHERE creado_por ~ '^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_canal_usuarios_id_propietario",
                schema: "comunicacion",
                table: "canal");

            migrationBuilder.DropIndex(
                name: "ix_canal_id_propietario",
                schema: "comunicacion",
                table: "canal");

            migrationBuilder.DropColumn(
                name: "es_publico",
                schema: "comunicacion",
                table: "canal");

            migrationBuilder.DropColumn(
                name: "id_propietario",
                schema: "comunicacion",
                table: "canal");
        }
    }
}

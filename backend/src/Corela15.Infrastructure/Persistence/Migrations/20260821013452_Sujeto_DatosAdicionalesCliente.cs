using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Sujeto_DatosAdicionalesCliente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "cobra_bono_desarrollo_humano",
                schema: "sujeto",
                table: "persona_natural",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "es_separacion_de_bienes",
                schema: "sujeto",
                table: "persona_natural",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "numero_cargas_familiares",
                schema: "sujeto",
                table: "persona_natural",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "tiene_cargas_familiares",
                schema: "sujeto",
                table: "persona_natural",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "tiene_discapacidad",
                schema: "sujeto",
                table: "persona_natural",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "calle_secundaria",
                schema: "sujeto",
                table: "persona",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "codigo_postal",
                schema: "sujeto",
                table: "persona",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "parentesco_servicio_basico",
                schema: "sujeto",
                table: "persona",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "referencia",
                schema: "sujeto",
                table: "persona",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "es_exento",
                schema: "clientes",
                table: "cliente",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "ix_cliente_id_usuario_oficial",
                schema: "clientes",
                table: "cliente",
                column: "id_usuario_oficial");

            migrationBuilder.AddForeignKey(
                name: "fk_cliente_usuarios_id_usuario_oficial",
                schema: "clientes",
                table: "cliente",
                column: "id_usuario_oficial",
                principalSchema: "seguridad",
                principalTable: "usuario",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_cliente_usuarios_id_usuario_oficial",
                schema: "clientes",
                table: "cliente");

            migrationBuilder.DropIndex(
                name: "ix_cliente_id_usuario_oficial",
                schema: "clientes",
                table: "cliente");

            migrationBuilder.DropColumn(
                name: "cobra_bono_desarrollo_humano",
                schema: "sujeto",
                table: "persona_natural");

            migrationBuilder.DropColumn(
                name: "es_separacion_de_bienes",
                schema: "sujeto",
                table: "persona_natural");

            migrationBuilder.DropColumn(
                name: "numero_cargas_familiares",
                schema: "sujeto",
                table: "persona_natural");

            migrationBuilder.DropColumn(
                name: "tiene_cargas_familiares",
                schema: "sujeto",
                table: "persona_natural");

            migrationBuilder.DropColumn(
                name: "tiene_discapacidad",
                schema: "sujeto",
                table: "persona_natural");

            migrationBuilder.DropColumn(
                name: "calle_secundaria",
                schema: "sujeto",
                table: "persona");

            migrationBuilder.DropColumn(
                name: "codigo_postal",
                schema: "sujeto",
                table: "persona");

            migrationBuilder.DropColumn(
                name: "parentesco_servicio_basico",
                schema: "sujeto",
                table: "persona");

            migrationBuilder.DropColumn(
                name: "referencia",
                schema: "sujeto",
                table: "persona");

            migrationBuilder.DropColumn(
                name: "es_exento",
                schema: "clientes",
                table: "cliente");
        }
    }
}

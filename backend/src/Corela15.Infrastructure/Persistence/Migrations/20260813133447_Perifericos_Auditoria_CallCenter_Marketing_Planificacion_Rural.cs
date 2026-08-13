using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Perifericos_Auditoria_CallCenter_Marketing_Planificacion_Rural : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "auditoria");

            migrationBuilder.EnsureSchema(
                name: "callcenter");

            migrationBuilder.EnsureSchema(
                name: "planificacion");

            migrationBuilder.EnsureSchema(
                name: "marketing");

            migrationBuilder.EnsureSchema(
                name: "herramientarural");

            migrationBuilder.CreateTable(
                name: "area_auditoria",
                schema: "auditoria",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_area_auditoria", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "indicador",
                schema: "planificacion",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_indicador", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "planificacion_anual",
                schema: "planificacion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    anio = table.Column<int>(type: "integer", nullable: false),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_planificacion_anual", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rifa",
                schema: "marketing",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    fecha_inicio = table.Column<DateOnly>(type: "date", nullable: false),
                    fecha_sorteo = table.Column<DateOnly>(type: "date", nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_rifa", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tipo_comentario",
                schema: "callcenter",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tipo_comentario", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tipo_producto_agrario",
                schema: "herramientarural",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tipo_producto_agrario", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "seguimiento",
                schema: "auditoria",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_area_auditoria = table.Column<int>(type: "integer", nullable: false),
                    descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    id_nivel_riesgo = table.Column<int>(type: "integer", nullable: false),
                    fecha_identificacion = table.Column<DateOnly>(type: "date", nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_seguimiento", x => x.id);
                    table.ForeignKey(
                        name: "fk_seguimiento_area_auditoria_id_area_auditoria",
                        column: x => x.id_area_auditoria,
                        principalSchema: "auditoria",
                        principalTable: "area_auditoria",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_seguimiento_nivel_riesgo_id_nivel_riesgo",
                        column: x => x.id_nivel_riesgo,
                        principalSchema: "riesgo",
                        principalTable: "nivel_riesgo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "rifa_premio",
                schema: "marketing",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_rifa = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    valor_referencial = table.Column<decimal>(type: "numeric(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_rifa_premio", x => x.id);
                    table.ForeignKey(
                        name: "fk_rifa_premio_rifa_id_rifa",
                        column: x => x.id_rifa,
                        principalSchema: "marketing",
                        principalTable: "rifa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "comentario",
                schema: "callcenter",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_cliente = table.Column<Guid>(type: "uuid", nullable: false),
                    id_tipo_comentario = table.Column<int>(type: "integer", nullable: false),
                    detalle = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_comentario", x => x.id);
                    table.ForeignKey(
                        name: "fk_comentario_cliente_id_cliente",
                        column: x => x.id_cliente,
                        principalSchema: "clientes",
                        principalTable: "cliente",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_comentario_tipos_comentario_id_tipo_comentario",
                        column: x => x.id_tipo_comentario,
                        principalSchema: "callcenter",
                        principalTable: "tipo_comentario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_comentario_id_cliente",
                schema: "callcenter",
                table: "comentario",
                column: "id_cliente");

            migrationBuilder.CreateIndex(
                name: "ix_comentario_id_tipo_comentario",
                schema: "callcenter",
                table: "comentario",
                column: "id_tipo_comentario");

            migrationBuilder.CreateIndex(
                name: "ix_planificacion_anual_anio",
                schema: "planificacion",
                table: "planificacion_anual",
                column: "anio",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_rifa_premio_id_rifa",
                schema: "marketing",
                table: "rifa_premio",
                column: "id_rifa");

            migrationBuilder.CreateIndex(
                name: "ix_seguimiento_id_area_auditoria",
                schema: "auditoria",
                table: "seguimiento",
                column: "id_area_auditoria");

            migrationBuilder.CreateIndex(
                name: "ix_seguimiento_id_nivel_riesgo",
                schema: "auditoria",
                table: "seguimiento",
                column: "id_nivel_riesgo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "comentario",
                schema: "callcenter");

            migrationBuilder.DropTable(
                name: "indicador",
                schema: "planificacion");

            migrationBuilder.DropTable(
                name: "planificacion_anual",
                schema: "planificacion");

            migrationBuilder.DropTable(
                name: "rifa_premio",
                schema: "marketing");

            migrationBuilder.DropTable(
                name: "seguimiento",
                schema: "auditoria");

            migrationBuilder.DropTable(
                name: "tipo_producto_agrario",
                schema: "herramientarural");

            migrationBuilder.DropTable(
                name: "tipo_comentario",
                schema: "callcenter");

            migrationBuilder.DropTable(
                name: "rifa",
                schema: "marketing");

            migrationBuilder.DropTable(
                name: "area_auditoria",
                schema: "auditoria");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Nivel7_TesoreriaYActivos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "activofijo");

            migrationBuilder.EnsureSchema(
                name: "proveeduria");

            migrationBuilder.EnsureSchema(
                name: "cuentasporcobrar");

            migrationBuilder.EnsureSchema(
                name: "portafolio");

            migrationBuilder.EnsureSchema(
                name: "obligacion");

            migrationBuilder.CreateTable(
                name: "activo",
                schema: "activofijo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    detalle = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    fecha_compra = table.Column<DateOnly>(type: "date", nullable: false),
                    valor = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    marca = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    modelo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    serie = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    es_vehiculo = table.Column<bool>(type: "boolean", nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_activo", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "articulo",
                schema: "proveeduria",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    marca = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_articulo", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "cuenta_por_cobrar",
                schema: "cuentasporcobrar",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    concepto = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    id_agencia = table.Column<int>(type: "integer", nullable: false),
                    id_persona = table.Column<Guid>(type: "uuid", nullable: false),
                    cuotas = table.Column<int>(type: "integer", nullable: false),
                    monto_inicial = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    saldo = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    fecha_creacion = table.Column<DateOnly>(type: "date", nullable: false),
                    fecha_vencimiento = table.Column<DateOnly>(type: "date", nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cuenta_por_cobrar", x => x.id);
                    table.ForeignKey(
                        name: "fk_cuenta_por_cobrar_agencia_id_agencia",
                        column: x => x.id_agencia,
                        principalSchema: "general",
                        principalTable: "agencia",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_cuenta_por_cobrar_personas_id_persona",
                        column: x => x.id_persona,
                        principalSchema: "sujeto",
                        principalTable: "persona",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "cuenta_por_pagar",
                schema: "cuentasporcobrar",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    concepto = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    id_agencia = table.Column<int>(type: "integer", nullable: false),
                    id_persona = table.Column<Guid>(type: "uuid", nullable: false),
                    monto_inicial = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    saldo = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    fecha_creacion = table.Column<DateOnly>(type: "date", nullable: false),
                    fecha_vencimiento = table.Column<DateOnly>(type: "date", nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cuenta_por_pagar", x => x.id);
                    table.ForeignKey(
                        name: "fk_cuenta_por_pagar_agencia_id_agencia",
                        column: x => x.id_agencia,
                        principalSchema: "general",
                        principalTable: "agencia",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_cuenta_por_pagar_personas_id_persona",
                        column: x => x.id_persona,
                        principalSchema: "sujeto",
                        principalTable: "persona",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "inversion_portafolio",
                schema: "portafolio",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    id_agencia = table.Column<int>(type: "integer", nullable: false),
                    institucion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    monto = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    tasa = table.Column<decimal>(type: "numeric(9,4)", nullable: false),
                    fecha_inversion = table.Column<DateOnly>(type: "date", nullable: false),
                    fecha_vencimiento = table.Column<DateOnly>(type: "date", nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inversion_portafolio", x => x.id);
                    table.ForeignKey(
                        name: "fk_inversion_portafolio_agencia_id_agencia",
                        column: x => x.id_agencia,
                        principalSchema: "general",
                        principalTable: "agencia",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "obligacion_financiera",
                schema: "obligacion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    numero_pagare = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    id_agencia = table.Column<int>(type: "integer", nullable: false),
                    deuda_inicial = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    valor_entregado = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    saldo_actual = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_obligacion_financiera", x => x.id);
                    table.ForeignKey(
                        name: "fk_obligacion_financiera_agencia_id_agencia",
                        column: x => x.id_agencia,
                        principalSchema: "general",
                        principalTable: "agencia",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_articulo_codigo",
                schema: "proveeduria",
                table: "articulo",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_cuenta_por_cobrar_id_agencia",
                schema: "cuentasporcobrar",
                table: "cuenta_por_cobrar",
                column: "id_agencia");

            migrationBuilder.CreateIndex(
                name: "ix_cuenta_por_cobrar_id_persona",
                schema: "cuentasporcobrar",
                table: "cuenta_por_cobrar",
                column: "id_persona");

            migrationBuilder.CreateIndex(
                name: "ix_cuenta_por_pagar_id_agencia",
                schema: "cuentasporcobrar",
                table: "cuenta_por_pagar",
                column: "id_agencia");

            migrationBuilder.CreateIndex(
                name: "ix_cuenta_por_pagar_id_persona",
                schema: "cuentasporcobrar",
                table: "cuenta_por_pagar",
                column: "id_persona");

            migrationBuilder.CreateIndex(
                name: "ix_inversion_portafolio_codigo",
                schema: "portafolio",
                table: "inversion_portafolio",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_inversion_portafolio_id_agencia",
                schema: "portafolio",
                table: "inversion_portafolio",
                column: "id_agencia");

            migrationBuilder.CreateIndex(
                name: "ix_obligacion_financiera_codigo",
                schema: "obligacion",
                table: "obligacion_financiera",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_obligacion_financiera_id_agencia",
                schema: "obligacion",
                table: "obligacion_financiera",
                column: "id_agencia");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "activo",
                schema: "activofijo");

            migrationBuilder.DropTable(
                name: "articulo",
                schema: "proveeduria");

            migrationBuilder.DropTable(
                name: "cuenta_por_cobrar",
                schema: "cuentasporcobrar");

            migrationBuilder.DropTable(
                name: "cuenta_por_pagar",
                schema: "cuentasporcobrar");

            migrationBuilder.DropTable(
                name: "inversion_portafolio",
                schema: "portafolio");

            migrationBuilder.DropTable(
                name: "obligacion_financiera",
                schema: "obligacion");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Colocacion_DiferimientoCastigoCustodia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "diferimiento_cuota",
                schema: "colocacion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_prestamo = table.Column<Guid>(type: "uuid", nullable: false),
                    dias_diferidos = table.Column<int>(type: "integer", nullable: false),
                    comentario = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    registrado_por = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_diferimiento_cuota", x => x.id);
                    table.ForeignKey(
                        name: "fk_diferimiento_cuota_prestamos_id_prestamo",
                        column: x => x.id_prestamo,
                        principalSchema: "colocacion",
                        principalTable: "prestamo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "estado_custodio_pagare",
                schema: "colocacion",
                columns: table => new
                {
                    codigo = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_estado_custodio_pagare", x => x.codigo);
                });

            // Catálogo real — verificado contra COLOCACION.ESTADO_CUSTODIO_PAGARE (2 filas).
            migrationBuilder.Sql(@"
INSERT INTO colocacion.estado_custodio_pagare (codigo, nombre, activo) VALUES
('E', 'Entregado', true),
('R', 'Receptado', true);
");

            migrationBuilder.CreateTable(
                name: "prestamo_castigado",
                schema: "colocacion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_prestamo = table.Column<Guid>(type: "uuid", nullable: false),
                    saldo_transferido = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    comentario = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    registrado_por = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    id_comprobante = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_prestamo_castigado", x => x.id);
                    table.ForeignKey(
                        name: "fk_prestamo_castigado_prestamos_id_prestamo",
                        column: x => x.id_prestamo,
                        principalSchema: "colocacion",
                        principalTable: "prestamo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "pagare_custodia",
                schema: "colocacion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_prestamo = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo_estado = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    ubicacion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    fecha_actualizacion = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pagare_custodia", x => x.id);
                    table.ForeignKey(
                        name: "fk_pagare_custodia_estado_custodio_pagare_codigo_estado",
                        column: x => x.codigo_estado,
                        principalSchema: "colocacion",
                        principalTable: "estado_custodio_pagare",
                        principalColumn: "codigo",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_pagare_custodia_prestamos_id_prestamo",
                        column: x => x.id_prestamo,
                        principalSchema: "colocacion",
                        principalTable: "prestamo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "pagare_custodia_movimiento",
                schema: "colocacion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_pagare_custodia = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo_estado = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    ubicacion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    es_recepcion = table.Column<bool>(type: "boolean", nullable: false),
                    fecha = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    registrado_por = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pagare_custodia_movimiento", x => x.id);
                    table.ForeignKey(
                        name: "fk_pagare_custodia_movimiento_pagare_custodia_id_pagare_custod",
                        column: x => x.id_pagare_custodia,
                        principalSchema: "colocacion",
                        principalTable: "pagare_custodia",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_diferimiento_cuota_id_prestamo",
                schema: "colocacion",
                table: "diferimiento_cuota",
                column: "id_prestamo");

            migrationBuilder.CreateIndex(
                name: "ix_pagare_custodia_codigo_estado",
                schema: "colocacion",
                table: "pagare_custodia",
                column: "codigo_estado");

            migrationBuilder.CreateIndex(
                name: "ix_pagare_custodia_id_prestamo",
                schema: "colocacion",
                table: "pagare_custodia",
                column: "id_prestamo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_pagare_custodia_movimiento_id_pagare_custodia",
                schema: "colocacion",
                table: "pagare_custodia_movimiento",
                column: "id_pagare_custodia");

            migrationBuilder.CreateIndex(
                name: "ix_prestamo_castigado_id_prestamo",
                schema: "colocacion",
                table: "prestamo_castigado",
                column: "id_prestamo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "diferimiento_cuota",
                schema: "colocacion");

            migrationBuilder.DropTable(
                name: "pagare_custodia_movimiento",
                schema: "colocacion");

            migrationBuilder.DropTable(
                name: "prestamo_castigado",
                schema: "colocacion");

            migrationBuilder.DropTable(
                name: "pagare_custodia",
                schema: "colocacion");

            migrationBuilder.DropTable(
                name: "estado_custodio_pagare",
                schema: "colocacion");
        }
    }
}

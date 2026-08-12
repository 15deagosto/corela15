using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Nivel6_Nomina : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "nomina");

            migrationBuilder.CreateTable(
                name: "empleado",
                schema: "nomina",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_persona = table.Column<Guid>(type: "uuid", nullable: false),
                    id_agencia = table.Column<int>(type: "integer", nullable: false),
                    cargo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    fecha_ingreso = table.Column<DateOnly>(type: "date", nullable: false),
                    recibe_fondos_reserva = table.Column<bool>(type: "boolean", nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_empleado", x => x.id);
                    table.ForeignKey(
                        name: "fk_empleado_agencia_id_agencia",
                        column: x => x.id_agencia,
                        principalSchema: "general",
                        principalTable: "agencia",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_empleado_personas_id_persona",
                        column: x => x.id_persona,
                        principalSchema: "sujeto",
                        principalTable: "persona",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "rol_pagos",
                schema: "nomina",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    periodo = table.Column<DateOnly>(type: "date", nullable: false),
                    tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_rol_pagos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rol_pagos_empleado",
                schema: "nomina",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_rol_pagos = table.Column<Guid>(type: "uuid", nullable: false),
                    id_empleado = table.Column<Guid>(type: "uuid", nullable: false),
                    ingresos = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    egresos = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    total = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    dias_laborados = table.Column<int>(type: "integer", nullable: false),
                    anulado = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_rol_pagos_empleado", x => x.id);
                    table.ForeignKey(
                        name: "fk_rol_pagos_empleado_empleado_id_empleado",
                        column: x => x.id_empleado,
                        principalSchema: "nomina",
                        principalTable: "empleado",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_rol_pagos_empleado_rol_pagos_id_rol_pagos",
                        column: x => x.id_rol_pagos,
                        principalSchema: "nomina",
                        principalTable: "rol_pagos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_empleado_id_agencia",
                schema: "nomina",
                table: "empleado",
                column: "id_agencia");

            migrationBuilder.CreateIndex(
                name: "ix_empleado_id_persona",
                schema: "nomina",
                table: "empleado",
                column: "id_persona",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_rol_pagos_periodo_tipo",
                schema: "nomina",
                table: "rol_pagos",
                columns: new[] { "periodo", "tipo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_rol_pagos_empleado_id_empleado",
                schema: "nomina",
                table: "rol_pagos_empleado",
                column: "id_empleado");

            migrationBuilder.CreateIndex(
                name: "ix_rol_pagos_empleado_id_rol_pagos_id_empleado",
                schema: "nomina",
                table: "rol_pagos_empleado",
                columns: new[] { "id_rol_pagos", "id_empleado" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "rol_pagos_empleado",
                schema: "nomina");

            migrationBuilder.DropTable(
                name: "empleado",
                schema: "nomina");

            migrationBuilder.DropTable(
                name: "rol_pagos",
                schema: "nomina");
        }
    }
}

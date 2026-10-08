using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Nomina_RolPagosExcelDetalladoYCodigoSoftbank : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "actas_finiquito",
                schema: "nomina",
                table: "rol_pagos_empleado",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "anticipo_sueldo",
                schema: "nomina",
                table: "rol_pagos_empleado",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "aporte_individual_iess",
                schema: "nomina",
                table: "rol_pagos_empleado",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "atrasos",
                schema: "nomina",
                table: "rol_pagos_empleado",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "bonificaciones",
                schema: "nomina",
                table: "rol_pagos_empleado",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "comisiones",
                schema: "nomina",
                table: "rol_pagos_empleado",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "componente_salarial",
                schema: "nomina",
                table: "rol_pagos_empleado",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "decimo_cuarto_acumula",
                schema: "nomina",
                table: "rol_pagos_empleado",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "decimo_cuarto_valor_acumulado",
                schema: "nomina",
                table: "rol_pagos_empleado",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "decimo_cuarto_valor_mensual",
                schema: "nomina",
                table: "rol_pagos_empleado",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "decimo_tercero_acumula",
                schema: "nomina",
                table: "rol_pagos_empleado",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "decimo_tercero_valor_acumulado",
                schema: "nomina",
                table: "rol_pagos_empleado",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "decimo_tercero_valor_mensual",
                schema: "nomina",
                table: "rol_pagos_empleado",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "descuentos",
                schema: "nomina",
                table: "rol_pagos_empleado",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "fondos_reserva_acumula",
                schema: "nomina",
                table: "rol_pagos_empleado",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "fondos_reserva_valor_acumulado",
                schema: "nomina",
                table: "rol_pagos_empleado",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "fondos_reserva_valor_mensual",
                schema: "nomina",
                table: "rol_pagos_empleado",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "horas_extraordinarias100",
                schema: "nomina",
                table: "rol_pagos_empleado",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "horas_extraordinarias50",
                schema: "nomina",
                table: "rol_pagos_empleado",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "horas_suplementarias100",
                schema: "nomina",
                table: "rol_pagos_empleado",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "horas_suplementarias50",
                schema: "nomina",
                table: "rol_pagos_empleado",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "movilizacion",
                schema: "nomina",
                table: "rol_pagos_empleado",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "prestamo_hipotecario",
                schema: "nomina",
                table: "rol_pagos_empleado",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "prestamo_quirografario",
                schema: "nomina",
                table: "rol_pagos_empleado",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "retencion_renta",
                schema: "nomina",
                table: "rol_pagos_empleado",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "sanciones",
                schema: "nomina",
                table: "rol_pagos_empleado",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "subsidio_iess",
                schema: "nomina",
                table: "rol_pagos_empleado",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "sueldo_afiliado",
                schema: "nomina",
                table: "rol_pagos_empleado",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "sueldo_proporcional",
                schema: "nomina",
                table: "rol_pagos_empleado",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "codigo_empleado_softbank",
                schema: "nomina",
                table: "empleado",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_empleado_codigo_empleado_softbank",
                schema: "nomina",
                table: "empleado",
                column: "codigo_empleado_softbank",
                unique: true,
                filter: "codigo_empleado_softbank IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_empleado_codigo_empleado_softbank",
                schema: "nomina",
                table: "empleado");

            migrationBuilder.DropColumn(
                name: "actas_finiquito",
                schema: "nomina",
                table: "rol_pagos_empleado");

            migrationBuilder.DropColumn(
                name: "anticipo_sueldo",
                schema: "nomina",
                table: "rol_pagos_empleado");

            migrationBuilder.DropColumn(
                name: "aporte_individual_iess",
                schema: "nomina",
                table: "rol_pagos_empleado");

            migrationBuilder.DropColumn(
                name: "atrasos",
                schema: "nomina",
                table: "rol_pagos_empleado");

            migrationBuilder.DropColumn(
                name: "bonificaciones",
                schema: "nomina",
                table: "rol_pagos_empleado");

            migrationBuilder.DropColumn(
                name: "comisiones",
                schema: "nomina",
                table: "rol_pagos_empleado");

            migrationBuilder.DropColumn(
                name: "componente_salarial",
                schema: "nomina",
                table: "rol_pagos_empleado");

            migrationBuilder.DropColumn(
                name: "decimo_cuarto_acumula",
                schema: "nomina",
                table: "rol_pagos_empleado");

            migrationBuilder.DropColumn(
                name: "decimo_cuarto_valor_acumulado",
                schema: "nomina",
                table: "rol_pagos_empleado");

            migrationBuilder.DropColumn(
                name: "decimo_cuarto_valor_mensual",
                schema: "nomina",
                table: "rol_pagos_empleado");

            migrationBuilder.DropColumn(
                name: "decimo_tercero_acumula",
                schema: "nomina",
                table: "rol_pagos_empleado");

            migrationBuilder.DropColumn(
                name: "decimo_tercero_valor_acumulado",
                schema: "nomina",
                table: "rol_pagos_empleado");

            migrationBuilder.DropColumn(
                name: "decimo_tercero_valor_mensual",
                schema: "nomina",
                table: "rol_pagos_empleado");

            migrationBuilder.DropColumn(
                name: "descuentos",
                schema: "nomina",
                table: "rol_pagos_empleado");

            migrationBuilder.DropColumn(
                name: "fondos_reserva_acumula",
                schema: "nomina",
                table: "rol_pagos_empleado");

            migrationBuilder.DropColumn(
                name: "fondos_reserva_valor_acumulado",
                schema: "nomina",
                table: "rol_pagos_empleado");

            migrationBuilder.DropColumn(
                name: "fondos_reserva_valor_mensual",
                schema: "nomina",
                table: "rol_pagos_empleado");

            migrationBuilder.DropColumn(
                name: "horas_extraordinarias100",
                schema: "nomina",
                table: "rol_pagos_empleado");

            migrationBuilder.DropColumn(
                name: "horas_extraordinarias50",
                schema: "nomina",
                table: "rol_pagos_empleado");

            migrationBuilder.DropColumn(
                name: "horas_suplementarias100",
                schema: "nomina",
                table: "rol_pagos_empleado");

            migrationBuilder.DropColumn(
                name: "horas_suplementarias50",
                schema: "nomina",
                table: "rol_pagos_empleado");

            migrationBuilder.DropColumn(
                name: "movilizacion",
                schema: "nomina",
                table: "rol_pagos_empleado");

            migrationBuilder.DropColumn(
                name: "prestamo_hipotecario",
                schema: "nomina",
                table: "rol_pagos_empleado");

            migrationBuilder.DropColumn(
                name: "prestamo_quirografario",
                schema: "nomina",
                table: "rol_pagos_empleado");

            migrationBuilder.DropColumn(
                name: "retencion_renta",
                schema: "nomina",
                table: "rol_pagos_empleado");

            migrationBuilder.DropColumn(
                name: "sanciones",
                schema: "nomina",
                table: "rol_pagos_empleado");

            migrationBuilder.DropColumn(
                name: "subsidio_iess",
                schema: "nomina",
                table: "rol_pagos_empleado");

            migrationBuilder.DropColumn(
                name: "sueldo_afiliado",
                schema: "nomina",
                table: "rol_pagos_empleado");

            migrationBuilder.DropColumn(
                name: "sueldo_proporcional",
                schema: "nomina",
                table: "rol_pagos_empleado");

            migrationBuilder.DropColumn(
                name: "codigo_empleado_softbank",
                schema: "nomina",
                table: "empleado");
        }
    }
}

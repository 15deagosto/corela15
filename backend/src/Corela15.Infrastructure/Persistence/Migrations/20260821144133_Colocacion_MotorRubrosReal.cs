using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Colocacion_MotorRubrosReal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_rubro_codigo",
                schema: "colocacion",
                table: "rubro");

            migrationBuilder.DropColumn(
                name: "codigo",
                schema: "colocacion",
                table: "rubro");

            migrationBuilder.AlterColumn<string>(
                name: "nombre",
                schema: "colocacion",
                table: "rubro",
                type: "character varying(300)",
                maxLength: 300,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "codigo_tipo_rubro",
                schema: "colocacion",
                table: "rubro",
                type: "character varying(20)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "es_adicional",
                schema: "colocacion",
                table: "rubro",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "es_calculo_adicional",
                schema: "colocacion",
                table: "rubro",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "es_condonacion",
                schema: "colocacion",
                table: "rubro",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "genera_adjudicacion",
                schema: "colocacion",
                table: "rubro",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "se_factura",
                schema: "colocacion",
                table: "rubro",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "tarifa_impuesto",
                schema: "colocacion",
                table: "rubro",
                type: "numeric(9,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "prestamo_rubro_cuenta_por_cobrar",
                schema: "colocacion",
                columns: table => new
                {
                    id_prestamo_rubro = table.Column<Guid>(type: "uuid", nullable: false),
                    id_cuenta_por_cobrar = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_prestamo_rubro_cuenta_por_cobrar", x => x.id_prestamo_rubro);
                    table.ForeignKey(
                        name: "fk_prestamo_rubro_cuenta_por_cobrar_cuenta_por_cobrar_id_cuent",
                        column: x => x.id_cuenta_por_cobrar,
                        principalSchema: "cuentasporcobrar",
                        principalTable: "cuenta_por_cobrar",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_prestamo_rubro_cuenta_por_cobrar_prestamo_rubro_id_prestamo",
                        column: x => x.id_prestamo_rubro,
                        principalSchema: "colocacion",
                        principalTable: "prestamo_rubro",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tipo_rubro",
                schema: "colocacion",
                columns: table => new
                {
                    codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    es_capital = table.Column<bool>(type: "boolean", nullable: false),
                    es_tasa = table.Column<bool>(type: "boolean", nullable: false),
                    es_mora = table.Column<bool>(type: "boolean", nullable: false),
                    es_seguro = table.Column<bool>(type: "boolean", nullable: false),
                    es_castigo = table.Column<bool>(type: "boolean", nullable: false),
                    es_adicional = table.Column<bool>(type: "boolean", nullable: false),
                    es_judicial = table.Column<bool>(type: "boolean", nullable: false),
                    es_prejudicial = table.Column<bool>(type: "boolean", nullable: false),
                    es_medico = table.Column<bool>(type: "boolean", nullable: false),
                    es_ahorros = table.Column<bool>(type: "boolean", nullable: false),
                    es_seguro_vehicular = table.Column<bool>(type: "boolean", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tipo_rubro", x => x.codigo);
                });

            migrationBuilder.CreateIndex(
                name: "ix_rubro_codigo_tipo_rubro",
                schema: "colocacion",
                table: "rubro",
                column: "codigo_tipo_rubro");

            migrationBuilder.CreateIndex(
                name: "ix_prestamo_rubro_cuenta_por_cobrar_id_cuenta_por_cobrar",
                schema: "colocacion",
                table: "prestamo_rubro_cuenta_por_cobrar",
                column: "id_cuenta_por_cobrar",
                unique: true);

            // Catálogo real de 12 tipos de rubro — verificado contra
            // COLOCACION.TIPO_RUBRO. Las banderas EsCapital/EsTasa/... acá
            // reemplazan el campo "Rubro.Codigo" inventado que tenía este
            // core antes (Softbank no tiene ese campo — identifica capital/
            // interés por el tipo compartido, no por un código propio de
            // cada rubro individual).
            migrationBuilder.Sql("""
                INSERT INTO colocacion.tipo_rubro
                    (codigo, nombre, es_capital, es_tasa, es_mora, es_seguro, es_castigo, es_adicional, es_judicial, es_prejudicial, es_medico, es_ahorros, es_seguro_vehicular, activo) VALUES
                ('AHO', 'Ahorros',            false, false, false, false, false, false, false, false, false, true,  false, true),
                ('CAP', 'Capital',            true,  false, false, false, false, false, false, false, false, false, false, true),
                ('CAS', 'Castigo',            false, false, false, false, true,  false, false, false, false, false, false, true),
                ('DIF', 'Diferidos',          false, false, false, false, false, false, false, false, false, false, false, true),
                ('GAJ', 'Gastos Judiciales',  false, false, false, false, false, true,  false, false, false, false, false, true),
                ('INT', 'Interés',            false, true,  false, false, false, false, false, false, false, false, false, true),
                ('MOR', 'Mora',               false, false, true,  false, false, false, false, false, false, false, false, true),
                ('OTR', 'Otros',              false, false, false, false, false, true,  false, false, false, false, false, true),
                ('PRE', 'Gastos de Cobranza', false, false, false, false, false, false, false, true,  false, false, false, true),
                ('SEG', 'Seguro Desgravamen', false, false, false, true,  false, false, false, false, false, false, false, true),
                ('SME', 'Seguro Médico',      false, false, false, false, false, false, false, false, true,  false, false, true),
                ('SVE', 'Seguro Vehicular',   false, false, false, false, false, false, false, false, false, false, true,  true);
                """);

            // Backfill de los 4 rubros ya sembrados desde Nivel 3 (ids 1-4,
            // en uso real por el préstamo vigente del ambiente de
            // desarrollo — nunca se reinsertan con id nuevo, solo se les
            // agrega su clasificación real). Corrección real encontrada:
            // "Seguro" tenía es_cuenta_por_cobrar=true sembrado sin base —
            // el real "Seguro Desgravamen" (COLOCACION.RUBRO id=5) tiene
            // ESCUENTAPORCOBRAR=false, corregido acá.
            migrationBuilder.Sql("""
                UPDATE colocacion.rubro SET codigo_tipo_rubro = 'CAP', genera_adjudicacion = true
                    WHERE nombre = 'Capital';
                UPDATE colocacion.rubro SET codigo_tipo_rubro = 'INT', es_condonacion = true
                    WHERE nombre = 'Interés';
                UPDATE colocacion.rubro SET codigo_tipo_rubro = 'MOR', es_condonacion = true
                    WHERE nombre = 'Mora';
                UPDATE colocacion.rubro SET codigo_tipo_rubro = 'SEG', nombre = 'Seguro Desgravamen',
                    es_calculo_adicional = true, es_cuenta_por_cobrar = false, es_condonacion = true
                    WHERE nombre = 'Seguro';
                """);

            // Los 42 rubros reales restantes de COLOCACION.RUBRO (46 filas
            // reales totales, verificadas en vivo — 4 ya cubiertas arriba).
            // Activo/EsCuentaPorCobrar/OrdenDeCobro/SeFactura/EsAdicional/
            // EsCondonacion respetan exactamente la fuente real, incluidos
            // los rubros ya inactivos en Softbank (nunca se "activan" acá
            // solo porque suenan útiles).
            migrationBuilder.Sql("""
                INSERT INTO colocacion.rubro
                    (codigo_tipo_rubro, nombre, es_calculo_adicional, genera_adjudicacion, es_cuenta_por_cobrar, orden_de_cobro, activo, se_factura, tarifa_impuesto, es_adicional, es_condonacion) VALUES
                ('CAS', 'Capital Castigo',                    false, false, false, 1,   true,  false, 0, false, false),
                ('OTR', 'Citaciones',                         false, false, true,  5,   false, false, 0, false, true),
                ('PRE', 'Cobranzas Credito',                  false, false, false, 6,   true,  true,  0, false, true),
                ('GAJ', 'Certificado de Gravamen',            false, false, true,  8,   true,  false, 0, false, true),
                ('DIF', 'Refinanciado Interés, Mora, Otros',  false, true,  false, 10,  false, false, 0, false, false),
                ('GAJ', 'Notificaciones',                     false, false, true,  9,   true,  false, 0, false, true),
                ('GAJ', 'Inicio Demanda Judicial',            false, false, true,  11,  false, false, 0, false, true),
                ('OTR', 'Costo de Movilizacion',              false, false, true,  12,  false, false, 0, false, true),
                ('GAJ', 'Demanda Judicial',                   false, false, true,  13,  true,  false, 0, false, true),
                ('SME', 'Seguro Médico',                      false, false, false, 14,  false, false, 0, false, true),
                ('AHO', 'R. Ahorros',                         false, true,  false, -1,  true,  false, 0, false, true),
                ('PRE', 'IVA 12% Gastos de Cobranza',         false, false, false, 7,   false, false, 0, false, true),
                ('OTR', 'Liquidacion costas',                 false, false, true,  15,  true,  false, 0, false, true),
                ('OTR', 'Interes Diferidos',                  false, false, true,  16,  false, false, 0, false, true),
                ('OTR', 'Sindico de quiebra',                 false, false, true,  17,  true,  false, 0, false, true),
                ('OTR', 'Liquidación de capital e interés',   false, false, true,  18,  false, false, 0, false, true),
                ('OTR', 'Otros Gastos',                       false, false, true,  19,  false, false, 0, false, true),
                ('OTR', 'Levantamiento de Hipoteca',          false, false, true,  20,  false, false, 0, false, true),
                ('OTR', 'Gastos de Procuración Judicial',     false, false, true,  20,  false, false, 0, false, true),
                ('OTR', 'Empresa de Cobranza',                false, false, true,  21,  false, false, 0, false, true),
                ('SVE', 'Seguro Vehicular',                   false, true,  false, 100, false, false, 0, false, true),
                ('GAJ', 'Cobranzas',                          false, false, true,  104, true,  false, 0, false, true),
                ('PRE', 'Costos Judiciales No',               false, false, true,  0,   false, false, 0, false, true),
                ('PRE', 'Retencion Judicial',                 false, false, false, 0,   false, false, 0, false, true),
                ('OTR', 'Publicaciones en prensa',            false, true,  false, 0,   true,  false, 0, false, true),
                ('OTR', 'Seguro Médico',                      false, true,  false, 0,   false, false, 0, true,  true),
                ('OTR', 'Seguro Exequial',                    false, true,  false, 0,   true,  false, 0, true,  true),
                ('OTR', 'Presentación de demanda',            false, false, true,  0,   false, false, 0, false, true),
                ('OTR', 'Calificación de demanda',            false, false, true,  0,   false, false, 0, false, true),
                ('GAJ', 'Honorario de Abogados',               false, false, false, 0,  true,  false, 0, false, true),
                ('GAJ', 'Gastos',                             false, false, true,  12,  true,  false, 0, false, true),
                ('OTR', 'Certificado Gravamen Migracion',     false, false, false, 0,   false, false, 0, false, true),
                ('OTR', 'Honorario de Abogados Manual',       false, false, false, 0,   false, false, 0, false, true),
                ('OTR', 'Interés Reprogramado Microcrédito',  false, true,  false, 0,   true,  false, 0, false, true),
                ('OTR', 'Interés Reprogramado',               false, true,  false, 0,   true,  false, 0, false, true),
                ('OTR', 'Cobros Diferidos',                   false, true,  false, 0,   true,  false, 0, false, true),
                ('OTR', 'Notificaciones',                     false, false, false, 9,   true,  false, 0, false, true),
                ('OTR', 'Fondo Irrepartible de Reserva',      false, false, false, 0,   true,  false, 0, false, false),
                ('OTR', 'Interés Ley Solidaria',              false, false, false, 0,   true,  false, 0, false, true),
                ('OTR', 'Gastos Judiciales',                  false, false, true,  0,   true,  false, 0, false, true),
                ('OTR', 'Solca',                              false, false, false, 0,   true,  false, 0, false, true),
                ('OTR', 'Seguro desgravament A',              false, false, false, 4,   true,  false, 0, false, true);
                """);

            migrationBuilder.AddForeignKey(
                name: "fk_rubro_tipo_rubro_codigo_tipo_rubro",
                schema: "colocacion",
                table: "rubro",
                column: "codigo_tipo_rubro",
                principalSchema: "colocacion",
                principalTable: "tipo_rubro",
                principalColumn: "codigo",
                onDelete: ReferentialAction.Restrict);

            // PrestamoRubro.Estado usaba texto libre "Pendiente"/"Pagado"
            // (nunca relacionado con COLOCACION.ESTADO_PRESTAMORUBRO real) —
            // el motor ahora filtra por los códigos reales P/C. Sin este
            // backfill, cualquier PrestamoRubro ya sembrado antes de esta
            // migración (ej. la tabla de amortización de un préstamo real
            // ya desembolsado) quedaría invisible para PagarCuotaAsync.
            migrationBuilder.Sql("""
                UPDATE colocacion.prestamo_rubro SET estado = 'P' WHERE estado = 'Pendiente';
                UPDATE colocacion.prestamo_rubro SET estado = 'C' WHERE estado = 'Pagado';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Quita los 42 rubros reales sembrados por esta migración antes
            // de tocar columnas — los 4 originales (Capital/Interés/Mora/
            // Seguro Desgravamen) se quedan, solo pierden su clasificación.
            migrationBuilder.Sql("""
                DELETE FROM colocacion.rubro
                WHERE nombre NOT IN ('Capital', 'Interés', 'Mora', 'Seguro Desgravamen', 'Seguro');
                UPDATE colocacion.rubro SET nombre = 'Seguro' WHERE nombre = 'Seguro Desgravamen';
                UPDATE colocacion.prestamo_rubro SET estado = 'Pendiente' WHERE estado = 'P';
                UPDATE colocacion.prestamo_rubro SET estado = 'Pagado' WHERE estado = 'C';
                """);

            migrationBuilder.DropForeignKey(
                name: "fk_rubro_tipo_rubro_codigo_tipo_rubro",
                schema: "colocacion",
                table: "rubro");

            migrationBuilder.DropTable(
                name: "prestamo_rubro_cuenta_por_cobrar",
                schema: "colocacion");

            migrationBuilder.DropTable(
                name: "tipo_rubro",
                schema: "colocacion");

            migrationBuilder.DropIndex(
                name: "ix_rubro_codigo_tipo_rubro",
                schema: "colocacion",
                table: "rubro");

            migrationBuilder.DropColumn(
                name: "codigo_tipo_rubro",
                schema: "colocacion",
                table: "rubro");

            migrationBuilder.DropColumn(
                name: "es_adicional",
                schema: "colocacion",
                table: "rubro");

            migrationBuilder.DropColumn(
                name: "es_calculo_adicional",
                schema: "colocacion",
                table: "rubro");

            migrationBuilder.DropColumn(
                name: "es_condonacion",
                schema: "colocacion",
                table: "rubro");

            migrationBuilder.DropColumn(
                name: "genera_adjudicacion",
                schema: "colocacion",
                table: "rubro");

            migrationBuilder.DropColumn(
                name: "se_factura",
                schema: "colocacion",
                table: "rubro");

            migrationBuilder.DropColumn(
                name: "tarifa_impuesto",
                schema: "colocacion",
                table: "rubro");

            migrationBuilder.AlterColumn<string>(
                name: "nombre",
                schema: "colocacion",
                table: "rubro",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(300)",
                oldMaxLength: 300);

            migrationBuilder.AddColumn<string>(
                name: "codigo",
                schema: "colocacion",
                table: "rubro",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "ix_rubro_codigo",
                schema: "colocacion",
                table: "rubro",
                column: "codigo",
                unique: true);
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Nomina_BeneficiosSocialesReales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "cargo",
                schema: "nomina",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    nombre = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    se_prorratea_sueldo = table.Column<bool>(type: "boolean", nullable: false),
                    es_cargo_externo = table.Column<bool>(type: "boolean", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cargo", x => x.id);
                });

            // Catálogo real completo — verificado contra NOMINA.CARGO (90 filas,
            // IDs preservados idénticos a la fuente, incluyendo duplicados e
            // inactivos reales — mismo criterio de preservación de anomalías ya
            // aplicado al resto del proyecto).
            migrationBuilder.Sql(@"
INSERT INTO nomina.cargo (id, nombre, se_prorratea_sueldo, es_cargo_externo, activo) VALUES
(1, 'Gerente General', false, false, true),
(2, 'Asistente de Gerencia', false, false, true),
(3, 'Auditor Interno', false, false, true),
(4, 'Analista de TI', false, false, true),
(5, 'Jefe de Riesgos', false, false, true),
(6, 'Oficial de Microcredito', false, false, true),
(7, 'Jefe Administrativo Financiero', false, false, false),
(8, 'Contador', false, false, true),
(9, 'Asistente Contable', false, false, true),
(10, 'Tesorero', false, false, true),
(11, 'Asesor Jurídico', false, false, true),
(12, 'Oficial de Cumplimiento', false, false, true),
(13, 'Notificador', false, false, true),
(14, 'Notificador', false, false, false),
(15, 'Coordinador de Planificacion y Procesos', false, false, true),
(16, 'Jefe de Gestion del Talento Humano', false, false, true),
(17, 'Jefa de Gestión de la Calidad', false, false, false),
(18, 'Responsable de Tecnologias de la Informacion', false, false, true),
(19, 'Coordinador de Sistemas', false, false, true),
(20, 'Tëcnico Programador', false, false, true),
(21, 'Analista Programador', false, false, true),
(22, 'Oficial de Captaciones', false, false, true),
(23, 'Coordinador de Seguridad y Salud Ocupacional', false, false, false),
(24, 'Oficial de Balcon de Servicios', false, false, true),
(25, 'Auxiliar de servicios', false, false, true),
(26, 'Operador de Consola', false, false, false),
(27, 'Oficial de Cumplimiento Suplente', false, false, true),
(28, 'Asistente de Mercadeo', false, false, true),
(29, 'Jefe de Agencia', false, false, true),
(30, 'Jefe de Operaciones', false, false, true),
(31, 'Gestor de Cobranza', false, false, true),
(32, 'Asesor de Infraestructura', false, false, true),
(33, 'Jefe de Boveda', false, false, true),
(34, 'Asistente Operativo', false, false, true),
(35, 'Asistente de Soporte Tecnico', false, false, true),
(36, 'Asistente de Gestion Operativa', false, false, true),
(37, 'Analista de Gestion del Talento Humano', false, false, true),
(38, 'Auditora Informática', false, false, true),
(39, 'Recibidor Pagador', false, false, true),
(40, 'Chofer', false, false, false),
(41, 'Jefe de Creditos', false, false, true),
(42, 'Guardia de Seguridad', false, false, true),
(43, 'Asesor de captaciones D.P.F.', false, false, true),
(44, 'Asesor de Credito', false, false, true),
(45, 'Asistente de Credito', false, false, true),
(46, 'Custodio de Valores', false, false, true),
(47, 'Asistente Financiero', false, false, true),
(48, 'Asistente de Riesgos', false, false, true),
(49, 'Servicios Generales', false, false, true),
(50, 'Secretaria de Gerencia', false, false, true),
(51, 'Coordinador de Marketing', false, false, true),
(52, 'Asistente de Operaciones', false, false, true),
(53, 'Asistente de Call Center', false, false, true),
(54, 'Analista de Credito', false, false, true),
(55, 'Auditor Metodologico', false, false, false),
(56, 'Jefe de Cobranzas', false, false, true),
(57, 'Analista Control Interno', false, false, true),
(58, 'Verificador/Digitador de Fabrica de Crédito', false, false, true),
(59, 'Oficial de Cobranzas', false, false, true),
(60, 'Asistente de Captaciones', false, false, true),
(61, 'Jefe de Creditos', false, false, true),
(62, 'Coordinador de Cobranzas', false, false, true),
(63, 'Subgerente Administrativo Financiero', false, false, true),
(64, 'Analista de Desarrollo', false, false, true),
(65, 'Analista de Proyectos Y Desarrollos', false, false, true),
(66, 'Analista de Soporte', false, false, true),
(67, 'Asesor Legal', false, false, true),
(68, 'Asistente de Negocios y Marketing', false, false, true),
(69, 'Asistente Administrativo', false, false, true),
(70, 'Asistente de Auditoria', false, false, true),
(71, 'Asistente de Credito', false, false, true),
(72, 'Asistente de Seguridad Fisica', false, false, true),
(73, 'Asistente Financiero', false, false, true),
(74, 'Auxiliar de Archivos', false, false, true),
(75, 'Auxiliar de Creditos', false, false, true),
(76, 'Jefe de Agencia Backup', false, false, true),
(77, 'Oficial de Atencion al Socio y Usuario Financiero', false, false, true),
(78, 'Oficial de Credito', false, false, true),
(79, 'Tecnico en Seguridad y Salud', false, false, true),
(80, 'Supervisor de Operaciones', false, false, true),
(81, 'Supervisor de Operaciones Matriz', false, false, false),
(82, 'Supervisor de Atencion al Socio y Usuario Financiero', false, false, true),
(83, 'Operador Call Center', false, false, true),
(84, 'Jefe de Seguridad Fisica', false, false, true),
(85, 'Recaudador', false, false, true),
(86, 'Cajero Financiero', false, false, true),
(87, 'Servicio al Cliente', false, false, true),
(88, 'Asistente de Archivo', false, false, true),
(89, 'Asesor de Crédito y Cobranzas', false, false, true),
(90, 'ASISTENTE DEL DEPARTAMENTO LEGAL', false, false, true);
");

            migrationBuilder.AddColumn<int>(
                name: "id_cargo",
                schema: "nomina",
                table: "empleado",
                type: "integer",
                nullable: true);

            // Backfill de los 2 empleados de ejemplo ya sembrados (cargo libre
            // "ASESORA DE CAPTACIONES"/"CAJERO") a su cargo real más cercano del
            // catálogo oficial — cualquier otro valor libre que hubiera quedado
            // (no debería, solo hay 2 filas reales en este ambiente) cae en
            // Gerente General (1) como resguardo, nunca en un id inexistente.
            migrationBuilder.Sql(@"
UPDATE nomina.empleado SET id_cargo = CASE
    WHEN cargo ILIKE '%CAPTACIONES%' THEN 22
    WHEN cargo ILIKE '%CAJERO%' THEN 86
    ELSE 1
END;
");

            migrationBuilder.AlterColumn<int>(
                name: "id_cargo",
                schema: "nomina",
                table: "empleado",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "cargo",
                schema: "nomina",
                table: "empleado");

            migrationBuilder.CreateTable(
                name: "empleado_decimo_cuarto",
                schema: "nomina",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_empleado = table.Column<Guid>(type: "uuid", nullable: false),
                    proyectado = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    acumulado = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    pagado = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ultimo_devengo = table.Column<DateOnly>(type: "date", nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_empleado_decimo_cuarto", x => x.id);
                    table.ForeignKey(
                        name: "fk_empleado_decimo_cuarto_empleado_id_empleado",
                        column: x => x.id_empleado,
                        principalSchema: "nomina",
                        principalTable: "empleado",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "empleado_decimo_tercero",
                schema: "nomina",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_empleado = table.Column<Guid>(type: "uuid", nullable: false),
                    proyectado = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    acumulado = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    pagado = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ultimo_devengo = table.Column<DateOnly>(type: "date", nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_empleado_decimo_tercero", x => x.id);
                    table.ForeignKey(
                        name: "fk_empleado_decimo_tercero_empleado_id_empleado",
                        column: x => x.id_empleado,
                        principalSchema: "nomina",
                        principalTable: "empleado",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "empleado_fondos_reserva",
                schema: "nomina",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_empleado = table.Column<Guid>(type: "uuid", nullable: false),
                    proyectado = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    acumulado = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    pagado = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ultimo_devengo = table.Column<DateOnly>(type: "date", nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_empleado_fondos_reserva", x => x.id);
                    table.ForeignKey(
                        name: "fk_empleado_fondos_reserva_empleado_id_empleado",
                        column: x => x.id_empleado,
                        principalSchema: "nomina",
                        principalTable: "empleado",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "empleado_provision_vacacion",
                schema: "nomina",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_empleado = table.Column<Guid>(type: "uuid", nullable: false),
                    acumulado = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    pagado = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ultimo_devengo = table.Column<DateOnly>(type: "date", nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_empleado_provision_vacacion", x => x.id);
                    table.ForeignKey(
                        name: "fk_empleado_provision_vacacion_empleado_id_empleado",
                        column: x => x.id_empleado,
                        principalSchema: "nomina",
                        principalTable: "empleado",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "parametro_nomina",
                schema: "nomina",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    salario_basico_unificado = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_parametro_nomina", x => x.id);
                });

            // Salario Básico Unificado real 2026 (Ecuador) — valor de
            // referencia, actualizar cuando el Ministerio de Trabajo publique
            // el SBU de un año nuevo (mismo criterio ya usado para
            // `riesgo.parametro_liquidez`).
            migrationBuilder.Sql("INSERT INTO nomina.parametro_nomina (salario_basico_unificado) VALUES (470.00);");

            migrationBuilder.CreateTable(
                name: "empleado_provision_vacacion_detalle",
                schema: "nomina",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_provision_vacacion = table.Column<Guid>(type: "uuid", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    ultimo_sueldo = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    dias = table.Column<decimal>(type: "numeric(9,2)", nullable: false),
                    valor_anterior_provision = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    valor_actual_provision = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    valor_a_provisionar = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_empleado_provision_vacacion_detalle", x => x.id);
                    table.ForeignKey(
                        name: "fk_empleado_provision_vacacion_detalle_empleado_provision_vaca",
                        column: x => x.id_provision_vacacion,
                        principalSchema: "nomina",
                        principalTable: "empleado_provision_vacacion",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_empleado_id_cargo",
                schema: "nomina",
                table: "empleado",
                column: "id_cargo");

            migrationBuilder.CreateIndex(
                name: "ix_empleado_decimo_cuarto_id_empleado",
                schema: "nomina",
                table: "empleado_decimo_cuarto",
                column: "id_empleado",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_empleado_decimo_tercero_id_empleado",
                schema: "nomina",
                table: "empleado_decimo_tercero",
                column: "id_empleado",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_empleado_fondos_reserva_id_empleado",
                schema: "nomina",
                table: "empleado_fondos_reserva",
                column: "id_empleado",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_empleado_provision_vacacion_id_empleado",
                schema: "nomina",
                table: "empleado_provision_vacacion",
                column: "id_empleado",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_empleado_provision_vacacion_detalle_id_provision_vacacion",
                schema: "nomina",
                table: "empleado_provision_vacacion_detalle",
                column: "id_provision_vacacion");

            migrationBuilder.AddForeignKey(
                name: "fk_empleado_cargo_id_cargo",
                schema: "nomina",
                table: "empleado",
                column: "id_cargo",
                principalSchema: "nomina",
                principalTable: "cargo",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_empleado_cargo_id_cargo",
                schema: "nomina",
                table: "empleado");

            migrationBuilder.DropTable(
                name: "empleado_decimo_cuarto",
                schema: "nomina");

            migrationBuilder.DropTable(
                name: "empleado_decimo_tercero",
                schema: "nomina");

            migrationBuilder.DropTable(
                name: "empleado_fondos_reserva",
                schema: "nomina");

            migrationBuilder.DropTable(
                name: "empleado_provision_vacacion_detalle",
                schema: "nomina");

            migrationBuilder.DropTable(
                name: "parametro_nomina",
                schema: "nomina");

            migrationBuilder.DropTable(
                name: "empleado_provision_vacacion",
                schema: "nomina");

            migrationBuilder.DropTable(
                name: "cargo",
                schema: "nomina");

            migrationBuilder.DropIndex(
                name: "ix_empleado_id_cargo",
                schema: "nomina",
                table: "empleado");

            migrationBuilder.DropColumn(
                name: "id_cargo",
                schema: "nomina",
                table: "empleado");

            migrationBuilder.AddColumn<string>(
                name: "cargo",
                schema: "nomina",
                table: "empleado",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}

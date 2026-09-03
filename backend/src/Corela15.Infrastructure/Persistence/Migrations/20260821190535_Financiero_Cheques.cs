using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Financiero_Cheques : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "financiero");

            migrationBuilder.CreateTable(
                name: "banco",
                schema: "general",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    nombre = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    es_nacional = table.Column<bool>(type: "boolean", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_banco", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "cheque",
                schema: "financiero",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_banco = table.Column<int>(type: "integer", nullable: false),
                    cuenta_corriente = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    numero_cheque = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    valor = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    id_cuenta = table.Column<Guid>(type: "uuid", nullable: false),
                    id_agencia = table.Column<int>(type: "integer", nullable: false),
                    fecha_ingreso = table.Column<DateOnly>(type: "date", nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    modificado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    modificado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cheque", x => x.id);
                    table.ForeignKey(
                        name: "fk_cheque_agencia_id_agencia",
                        column: x => x.id_agencia,
                        principalSchema: "general",
                        principalTable: "agencia",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_cheque_banco_id_banco",
                        column: x => x.id_banco,
                        principalSchema: "general",
                        principalTable: "banco",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_cheque_cuentas_id_cuenta",
                        column: x => x.id_cuenta,
                        principalSchema: "ahorros",
                        principalTable: "cuenta",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "cheque_protesto",
                schema: "financiero",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_cheque = table.Column<Guid>(type: "uuid", nullable: false),
                    documento = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    fecha_proceso = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    registrado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cheque_protesto", x => x.id);
                    table.ForeignKey(
                        name: "fk_cheque_protesto_cheque_id_cheque",
                        column: x => x.id_cheque,
                        principalSchema: "financiero",
                        principalTable: "cheque",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_cheque_id_agencia",
                schema: "financiero",
                table: "cheque",
                column: "id_agencia");

            migrationBuilder.CreateIndex(
                name: "ix_cheque_id_banco",
                schema: "financiero",
                table: "cheque",
                column: "id_banco");

            migrationBuilder.CreateIndex(
                name: "ix_cheque_id_cuenta",
                schema: "financiero",
                table: "cheque",
                column: "id_cuenta");

            migrationBuilder.CreateIndex(
                name: "ix_cheque_protesto_id_cheque",
                schema: "financiero",
                table: "cheque_protesto",
                column: "id_cheque",
                unique: true);

            // Catálogo real completo de 89 bancos/instituciones —
            // verificado contra GENERAL.BANCO. IDs reales preservados
            // (no contiguos, huecos reales en la fuente) para trazabilidad
            // directa 1:1 con Softbank. El campo CODIGO tiene duplicados
            // reales en la fuente (ej. "100" se repite ~15 veces) — nunca
            // se fuerza unicidad que la fuente no tiene.
            migrationBuilder.Sql("""
                INSERT INTO general.banco (id, codigo, nombre, es_nacional, activo) VALUES
                (141, '18', 'Banco Bolivariano', true, true),
                (142, '17', 'Banco Guayaquil', true, true),
                (143, '99', 'Banco de Loja', true, false),
                (144, '9', 'Banco de Machala', true, false),
                (145, '16', 'Banco del Austro', true, true),
                (146, '10', 'Banco del Pichincha', true, true),
                (147, '21', 'Banco General Rumiñahui', true, true),
                (148, '14', 'Banco Internacional', true, true),
                (149, '1', 'Banco Nacional de Fomento', true, false),
                (150, '26', 'Banco Solidario', true, false),
                (151, '120', 'BanEcuador', true, true),
                (152, '121', 'Coop. CACPE Gualaquiza', true, false),
                (153, '122', 'Coop. CACPE Loja', true, false),
                (154, '123', 'Coop. CACPE Yantzaza', true, false),
                (155, '124', 'Coop. CACPE Zamora', true, false),
                (156, '125', 'Coop. Cadecol', true, false),
                (157, '126', 'Coop. CCQ', true, false),
                (158, '127', 'Coop. Crediamigo', true, false),
                (159, '128', 'Coop. Cristo Rey', true, false),
                (160, '129', 'Coop. Desarrollo', true, false),
                (161, '130', 'Coopmego', true, false),
                (162, '131', 'Coop. Nuevos Horizontes', true, false),
                (163, '132', 'Coop. Padre Julián Lorente', true, false),
                (164, '24', 'Diners', true, false),
                (165, '25', 'FINCA', true, false),
                (166, '26', 'IECE', true, false),
                (167, '27', 'IESS', true, false),
                (168, '28', 'MINGA', true, false),
                (169, '29', 'Mutualista Pichincha', true, false),
                (170, '60', 'ProCredit', true, true),
                (171, '36', 'Produbanco', true, true),
                (172, '320', 'Unibanco', true, false),
                (173, '33', 'Banco Central del Ecuador', true, true),
                (174, '34', 'Banco Desarrollo', true, true),
                (175, '62', 'Coop. JEP', true, false),
                (176, '33', 'COAC 23 de Julio', true, false),
                (177, '51', 'FINANCOOP', true, false),
                (178, '11', 'Banco del Pacífico', true, true),
                (179, '133', 'Citybank', false, false),
                (180, '134', 'Bancoamazonas', true, false),
                (181, '50', 'Banco Cofiec', true, false),
                (182, '140', 'Cooperativa de Ahorro y Crédito Indígena SAC Ltda.', true, false),
                (183, '150', 'Cooperativa Visión de los Andes Vis Andes', true, true),
                (184, '151', 'Cooperativa de Ahorro y Crédito 9 de Octubre Ltda.', true, false),
                (185, '152', 'Cooperativa Daquilema', true, true),
                (186, '153', 'Cooperativa de Ahorro y Crédito Chibuleo', true, false),
                (187, '153', 'Cooperativa de Ahorro y Crédito Ambato Ltda.', true, false),
                (188, '154', 'Cooperativa de Ahorro y Crédito Coopac Ltda.', true, false),
                (189, '155', 'Cooperativa de Ahorro y Crédito Educadores de Tungurahua Ltda.', true, false),
                (190, '156', 'Cooperativa de Ahorro y Crédito Virgen del Cisne', true, false),
                (191, '157', 'COAC. Líderes del Progreso', true, false),
                (192, '158', 'Cooperativa de Transporte Intercantonal Santa Ana', true, false),
                (193, '159', 'Cooperativa de Ahorro y Crédito Mushuc Runa Ltda.', true, false),
                (194, '160', 'Cooperativa de Ahorro y Crédito Acción Tungurahua', true, false),
                (195, '161', 'Cooperativa CCCA', true, false),
                (196, '162', 'Cooperativa de Ahorro y Crédito Manantial de Oro Ltda.', true, false),
                (197, '163', 'Cooperativa de Ahorro y Crédito Coopac Ltda.', true, false),
                (198, '164', 'Cooperativa de Ahorro y Crédito Uniotavalo Ltda.', true, true),
                (199, '165', 'Cooperativa de Ahorro y Crédito 1 de Junio', true, false),
                (200, '166', 'Cooperativa de Ahorro y Crédito Ecuafuturo Ltda.', true, false),
                (201, '167', 'Cooperativa de Ahorro y Crédito Sumak Sisa', true, false),
                (202, '168', 'Coop. de Ahorro y Crédito Ecuacréditos', true, false),
                (203, '200', 'Cooperativa de Ahorro y Crédito Maquita Cushun Ltda.', true, false),
                (204, '201', 'Cooperativa de Ahorro y Crédito Luz del Valle', true, false),
                (205, '202', 'Cooperativa de Ahorro y Crédito San Francisco Ltda.', true, true),
                (206, '203', 'Cooperativa de Ahorro y Crédito Puellaro Ltda.', true, false),
                (207, '100', 'Cooperativa de Ahorro y Crédito Cámara de Comercio de Santo Domingo', true, false),
                (208, '100', 'Cooperativa de Ahorro y Crédito Andalucía Ltda.', true, false),
                (209, '100', 'Cooperativa de Ahorro y Crédito Artesanos Ltda.', true, false),
                (218, '100', 'Cooperativa de Ahorro y Crédito Cotocollao Ltda.', true, false),
                (219, '100', 'Cooperativa de Ahorro y Crédito Credi Ya Ltda.', true, false),
                (220, '100', 'Cooperativa de Ahorro y Crédito Minga Ltda.', true, false),
                (221, '100', 'Cooperativa de Transporte de Pasajeros en Taxis Grecia', true, false),
                (222, '100', 'Cooperativa de Servicios Exequiales Solidaria', true, false),
                (223, '100', 'Cooperativa de Ahorro y Crédito San Juan de Cotogchoa', true, false),
                (224, '100', 'Cooperativa de Ahorro y Crédito Huaicana Ltda.', true, false),
                (225, '100', 'Cooperativa de Ahorro y Crédito Sumak Kawsay Ltda.', true, false),
                (226, '100', 'Banco Comercial de Manabí', true, false),
                (227, '101', 'Banco Exterior Pruebas', false, false),
                (228, '039', 'Cooperativa Chone', true, false),
                (229, '102', 'Cooperativa de Ahorro y Crédito CACPECO', true, true),
                (230, '100', 'Sivinta Sonia Maribel', true, false),
                (231, '100', 'Cooperativa de Ahorro y Crédito Occidental', true, true),
                (232, '100', 'Super Taxi Centro Comercial Aeropuerto Coop. de Transporte de Pasajeros en Taxis', true, true),
                (233, '250', 'Cooperativa de Ahorro y Crédito Guaranda', true, true),
                (234, '251', 'Cooperativa de Ahorro y Crédito CB Biblián', true, true),
                (235, '252', 'Cooperativa de Ahorro y Crédito Vencedores Ltda.', true, true),
                (236, '100', 'Caja de Ahorro Emprendedores Reina del Tránsito Cereitrans', true, true),
                (237, '100', 'Cooperativa de Ahorro y Crédito Pichincha Ltda.', true, true);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM general.banco WHERE id BETWEEN 141 AND 237;");

            migrationBuilder.DropTable(
                name: "cheque_protesto",
                schema: "financiero");

            migrationBuilder.DropTable(
                name: "cheque",
                schema: "financiero");

            migrationBuilder.DropTable(
                name: "banco",
                schema: "general");
        }
    }
}

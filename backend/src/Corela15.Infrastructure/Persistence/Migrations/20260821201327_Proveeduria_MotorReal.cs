using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Proveeduria_MotorReal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_articulo",
                schema: "proveeduria",
                table: "articulo");

            migrationBuilder.DropIndex(
                name: "ix_articulo_codigo",
                schema: "proveeduria",
                table: "articulo");

            migrationBuilder.DropColumn(
                name: "id",
                schema: "proveeduria",
                table: "articulo");

            migrationBuilder.AlterColumn<string>(
                name: "marca",
                schema: "proveeduria",
                table: "articulo",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "codigo_tipo_articulo",
                schema: "proveeduria",
                table: "articulo",
                type: "character varying(20)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "multiplo",
                schema: "proveeduria",
                table: "articulo",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "pk_articulo",
                schema: "proveeduria",
                table: "articulo",
                column: "codigo");

            migrationBuilder.CreateTable(
                name: "bodega",
                schema: "proveeduria",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_agencia = table.Column<int>(type: "integer", nullable: false),
                    id_usuario_responsable = table.Column<Guid>(type: "uuid", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_bodega", x => x.id);
                    table.ForeignKey(
                        name: "fk_bodega_agencia_id_agencia",
                        column: x => x.id_agencia,
                        principalSchema: "general",
                        principalTable: "agencia",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_bodega_usuarios_id_usuario_responsable",
                        column: x => x.id_usuario_responsable,
                        principalSchema: "seguridad",
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tipo_articulo",
                schema: "proveeduria",
                columns: table => new
                {
                    codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    detalle = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    id_cuenta_contable_activo = table.Column<Guid>(type: "uuid", nullable: false),
                    id_cuenta_contable_gasto = table.Column<Guid>(type: "uuid", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tipo_articulo", x => x.codigo);
                    table.ForeignKey(
                        name: "fk_tipo_articulo_cuenta_contable_id_cuenta_contable_activo",
                        column: x => x.id_cuenta_contable_activo,
                        principalSchema: "contabilidad",
                        principalTable: "cuenta_contable",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_tipo_articulo_cuenta_contable_id_cuenta_contable_gasto",
                        column: x => x.id_cuenta_contable_gasto,
                        principalSchema: "contabilidad",
                        principalTable: "cuenta_contable",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "bodega_articulo",
                schema: "proveeduria",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_bodega = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo_articulo = table.Column<string>(type: "character varying(20)", nullable: false),
                    cantidad = table.Column<int>(type: "integer", nullable: false),
                    precio_unitario = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    valor_total = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_bodega_articulo", x => x.id);
                    table.ForeignKey(
                        name: "fk_bodega_articulo_articulo_codigo_articulo",
                        column: x => x.codigo_articulo,
                        principalSchema: "proveeduria",
                        principalTable: "articulo",
                        principalColumn: "codigo",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_bodega_articulo_bodegas_id_bodega",
                        column: x => x.id_bodega,
                        principalSchema: "proveeduria",
                        principalTable: "bodega",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "solicitud_pedido",
                schema: "proveeduria",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_bodega = table.Column<Guid>(type: "uuid", nullable: false),
                    id_usuario_solicitante = table.Column<Guid>(type: "uuid", nullable: false),
                    detalle = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    fecha_sistema = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    fecha_proceso = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_solicitud_pedido", x => x.id);
                    table.ForeignKey(
                        name: "fk_solicitud_pedido_bodega_id_bodega",
                        column: x => x.id_bodega,
                        principalSchema: "proveeduria",
                        principalTable: "bodega",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_solicitud_pedido_usuarios_id_usuario_solicitante",
                        column: x => x.id_usuario_solicitante,
                        principalSchema: "seguridad",
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "bodega_articulo_movimiento",
                schema: "proveeduria",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_bodega_articulo = table.Column<Guid>(type: "uuid", nullable: false),
                    cantidad = table.Column<int>(type: "integer", nullable: false),
                    valor = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    saldo_resultante = table.Column<int>(type: "integer", nullable: false),
                    es_baja_articulo = table.Column<bool>(type: "boolean", nullable: false),
                    comentario = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    fecha_sistema = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    registrado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_bodega_articulo_movimiento", x => x.id);
                    table.ForeignKey(
                        name: "fk_bodega_articulo_movimiento_bodega_articulo_id_bodega_articu",
                        column: x => x.id_bodega_articulo,
                        principalSchema: "proveeduria",
                        principalTable: "bodega_articulo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "solicitud_pedido_articulo",
                schema: "proveeduria",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_solicitud = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo_articulo = table.Column<string>(type: "character varying(20)", nullable: false),
                    cantidad = table.Column<int>(type: "integer", nullable: false),
                    precio_unitario = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    detalle = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_solicitud_pedido_articulo", x => x.id);
                    table.ForeignKey(
                        name: "fk_solicitud_pedido_articulo_articulo_codigo_articulo",
                        column: x => x.codigo_articulo,
                        principalSchema: "proveeduria",
                        principalTable: "articulo",
                        principalColumn: "codigo",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_solicitud_pedido_articulo_solicitudes_pedido_id_solicitud",
                        column: x => x.id_solicitud,
                        principalSchema: "proveeduria",
                        principalTable: "solicitud_pedido",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "solicitud_pedido_etapa",
                schema: "proveeduria",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_solicitud = table.Column<Guid>(type: "uuid", nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    comentario = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    registrado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    fecha_sistema = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_solicitud_pedido_etapa", x => x.id);
                    table.ForeignKey(
                        name: "fk_solicitud_pedido_etapa_solicitud_pedido_id_solicitud",
                        column: x => x.id_solicitud,
                        principalSchema: "proveeduria",
                        principalTable: "solicitud_pedido",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_articulo_codigo_tipo_articulo",
                schema: "proveeduria",
                table: "articulo",
                column: "codigo_tipo_articulo");

            migrationBuilder.CreateIndex(
                name: "ix_bodega_id_agencia",
                schema: "proveeduria",
                table: "bodega",
                column: "id_agencia");

            migrationBuilder.CreateIndex(
                name: "ix_bodega_id_usuario_responsable",
                schema: "proveeduria",
                table: "bodega",
                column: "id_usuario_responsable");

            migrationBuilder.CreateIndex(
                name: "ix_bodega_articulo_codigo_articulo",
                schema: "proveeduria",
                table: "bodega_articulo",
                column: "codigo_articulo");

            migrationBuilder.CreateIndex(
                name: "ix_bodega_articulo_id_bodega_codigo_articulo",
                schema: "proveeduria",
                table: "bodega_articulo",
                columns: new[] { "id_bodega", "codigo_articulo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_bodega_articulo_movimiento_id_bodega_articulo",
                schema: "proveeduria",
                table: "bodega_articulo_movimiento",
                column: "id_bodega_articulo");

            migrationBuilder.CreateIndex(
                name: "ix_solicitud_pedido_id_bodega",
                schema: "proveeduria",
                table: "solicitud_pedido",
                column: "id_bodega");

            migrationBuilder.CreateIndex(
                name: "ix_solicitud_pedido_id_usuario_solicitante",
                schema: "proveeduria",
                table: "solicitud_pedido",
                column: "id_usuario_solicitante");

            migrationBuilder.CreateIndex(
                name: "ix_solicitud_pedido_articulo_codigo_articulo",
                schema: "proveeduria",
                table: "solicitud_pedido_articulo",
                column: "codigo_articulo");

            migrationBuilder.CreateIndex(
                name: "ix_solicitud_pedido_articulo_id_solicitud",
                schema: "proveeduria",
                table: "solicitud_pedido_articulo",
                column: "id_solicitud");

            migrationBuilder.CreateIndex(
                name: "ix_solicitud_pedido_etapa_id_solicitud",
                schema: "proveeduria",
                table: "solicitud_pedido_etapa",
                column: "id_solicitud");

            migrationBuilder.CreateIndex(
                name: "ix_tipo_articulo_id_cuenta_contable_activo",
                schema: "proveeduria",
                table: "tipo_articulo",
                column: "id_cuenta_contable_activo");

            migrationBuilder.CreateIndex(
                name: "ix_tipo_articulo_id_cuenta_contable_gasto",
                schema: "proveeduria",
                table: "tipo_articulo",
                column: "id_cuenta_contable_gasto");

            migrationBuilder.AddForeignKey(
                name: "fk_articulo_tipo_articulo_codigo_tipo_articulo",
                schema: "proveeduria",
                table: "articulo",
                column: "codigo_tipo_articulo",
                principalSchema: "proveeduria",
                principalTable: "tipo_articulo",
                principalColumn: "codigo",
                onDelete: ReferentialAction.Restrict);

            // PROVEEDURIA.TIPO_ARTICULO real (4 filas) — cada cuenta
            // contable real de 8 dígitos de Softbank se mapeó a la cuenta
            // oficial de 6 dígitos más cercana por semántica (nunca
            // inventada): activo siempre 190615 Proveeduría; gasto según
            // el tipo real (Publicidad para OI/P, Suministros diversos
            // para SO/UAL — no hay cuenta oficial más fina para aseo).
            migrationBuilder.Sql("""
                WITH cuentas AS (SELECT codigo, id FROM contabilidad.cuenta_contable)
                INSERT INTO proveeduria.tipo_articulo (codigo, nombre, detalle, id_cuenta_contable_activo, id_cuenta_contable_gasto, activo)
                SELECT v.codigo, v.nombre, v.detalle,
                    (SELECT id FROM cuentas WHERE codigo = '190615'),
                    (SELECT id FROM cuentas WHERE codigo = v.cta_gasto),
                    true
                FROM (VALUES
                    ('OI',  'Obsequios de inversiones',       'Promociones',              '450315'),
                    ('P',   'Publicidad',                     'Publicidad impresa',       '450315'),
                    ('SO',  'Suministros de oficina',         'Suministros de oficina',   '450705'),
                    ('UAL', 'Útiles de aseo y limpieza',      'Suministros de limpieza',  '450705')
                ) AS v(codigo, nombre, detalle, cta_gasto);
                """);

            // PROVEEDURIA.ARTICULO real (113 filas activas reales, código
            // propio como clave natural — verificado uno a uno contra la
            // fuente).
            migrationBuilder.Sql("""
                INSERT INTO proveeduria.articulo (codigo, nombre, codigo_tipo_articulo, marca, multiplo, activo) VALUES
                ('001', 'Apoyamano A4 madera', 'SO', 'Sin marca', 'UNIDAD', true),
                ('002', 'Esfero', 'SO', 'Sin marca', 'UNIDAD', true),
                ('003', 'Borrador', 'SO', 'Sin marca', 'UNIDAD', true),
                ('004', 'Carpeta cartulina cartón', 'SO', 'Sin marca', 'UNIDAD', true),
                ('005', 'Clips plateado', 'SO', 'Sin marca', 'UNIDAD', true),
                ('006', 'Corrector', 'SO', 'Sin marca', 'UNIDAD', true),
                ('007', 'Grapadora', 'SO', 'Sin marca', 'UNIDAD', true),
                ('008', 'Lápiz', 'SO', 'Sin marca', 'UNIDAD', true),
                ('009', 'Notitas', 'SO', 'Sin marca', 'UNIDAD', true),
                ('010', 'Perforadora', 'SO', 'Sin marca', 'UNIDAD', true),
                ('011', 'Portafolio elástico pequeño', 'SO', 'Sin marca', 'UNIDAD', true),
                ('012', 'Portafolio elástico grande', 'SO', 'Sin marca', 'UNIDAD', true),
                ('013', 'Regla plástica', 'SO', 'Sin marca', 'UNIDAD', true),
                ('014', 'Resaltador', 'SO', 'Sin marca', 'UNIDAD', true),
                ('015', 'Sacagrapa', 'SO', 'Sin marca', 'UNIDAD', true),
                ('016', 'Sacapunta', 'SO', 'Sin marca', 'UNIDAD', true),
                ('017', 'Separador hojas 10 colores', 'SO', 'Sin marca', 'UNIDAD', true),
                ('018', 'Tijera', 'SO', 'Sin marca', 'UNIDAD', true),
                ('019', 'Vinchas carpeta', 'SO', 'Sin marca', 'UNIDAD', true),
                ('020', 'Vinchas carpeta metal', 'SO', 'Sin marca', 'UNIDAD', true),
                ('021', 'Papeletas', 'SO', 'Sin marca', 'UNIDAD', true),
                ('022', 'Cartolas', 'SO', 'Sin marca', 'UNIDAD', true),
                ('023', 'Estuches', 'SO', 'Sin marca', 'UNIDAD', true),
                ('024', 'Arroba de arroz', 'OI', 'Arroz', 'UNIDAD', true),
                ('025', 'Tinta Epson 504 negro', 'SO', 'Epson', 'UNIDAD', true),
                ('026', 'Tinta Epson 504 cyan', 'SO', 'Epson', 'UNIDAD', true),
                ('027', 'Tinta Epson 504 yellow', 'SO', 'Epson', 'UNIDAD', true),
                ('028', 'Tinta Epson 504 magenta', 'SO', 'Epson', 'UNIDAD', true),
                ('029', 'Lustre muebles', 'UAL', 'Lustre', 'UNIDAD', true),
                ('030', 'Cloro', 'UAL', 'Cloro', 'UNIDAD', true),
                ('031', 'Ambiental', 'UAL', 'Ambiental', 'UNIDAD', true),
                ('032', 'Tips baño', 'UAL', 'Tips', 'UNIDAD', true),
                ('033', 'Tips ambiental', 'UAL', 'Tips', 'UNIDAD', true),
                ('034', 'Papel higiénico', 'UAL', 'Papel higiénico', 'UNIDAD', true),
                ('035', 'Funda negra de basura', 'UAL', 'Funda', 'UNIDAD', true),
                ('036', 'Funda floral basura', 'UAL', 'Floral', 'UNIDAD', true),
                ('037', 'Paño*22', 'UAL', 'Paño', 'UNIDAD', true),
                ('038', 'Escoba coco', 'UAL', 'Coco', 'UNIDAD', true),
                ('039', 'Escoba plástica', 'UAL', 'Plástica', 'UNIDAD', true),
                ('040', 'Trapeador de mopa plana', 'UAL', 'Trapeador', 'UNIDAD', true),
                ('041', 'Mopa plana', 'UAL', 'Mopa', 'UNIDAD', true),
                ('042', 'Pala plástica', 'UAL', 'Plástica', 'UNIDAD', true),
                ('043', 'Toalla mediana', 'UAL', 'Toalla', 'UNIDAD', true),
                ('044', 'Atomizador', 'UAL', 'Atomizador', 'UNIDAD', true),
                ('045', 'Cepillo sanitario con base', 'UAL', 'Cepillo', 'UNIDAD', true),
                ('046', 'Cepillo sanitario', 'UAL', 'Cepillo', 'UNIDAD', true),
                ('047', 'Caneca limpia vidrios', 'UAL', 'Limpia vidrios', 'GALON', true),
                ('048', 'Dispensador de papel higiénico', 'UAL', 'Dispensador', 'UNIDAD', true),
                ('049', 'Dispensador jabón líquido', 'UAL', 'Dispensador', 'UNIDAD', true),
                ('050', 'Botella para dispensador', 'UAL', 'Dispensador', 'UNIDAD', true),
                ('051', 'Caneca desinfectante floral', 'UAL', 'Floral', 'GALON', true),
                ('052', 'Candado', 'SO', 'Candado', 'UNIDAD', true),
                ('053', 'Archivador folder', 'SO', 'Folder', 'UNIDAD', true),
                ('054', 'Archivador doble anillo', 'SO', 'Doble anillo', 'UNIDAD', true),
                ('055', 'Cinta adhesiva pequeña', 'SO', 'Cinta', 'UNIDAD', true),
                ('056', 'Cinta adhesiva embalaje', 'SO', 'Cinta', 'UNIDAD', true),
                ('057', 'Cinta adhesiva masking', 'SO', 'Cinta', 'UNIDAD', true),
                ('058', 'Cinta impresora Epson', 'SO', 'Cinta', 'UNIDAD', true),
                ('059', 'Clips mariposa', 'SO', 'Mariposa', 'UNIDAD', true),
                ('060', 'Estilete metálico', 'SO', 'Metálico', 'UNIDAD', true),
                ('061', 'Estilete plástico', 'SO', 'Plástico', 'UNIDAD', true),
                ('062', 'Etiqueta multipeg', 'SO', 'Multipeg', 'FUNDA', true),
                ('063', 'Funda transparente rollo 6*8', 'SO', 'Transparente', 'ROLLO', true),
                ('064', 'Hoja adhesivas A4', 'SO', 'Adhesiva', 'UNIDAD', true),
                ('065', 'Papel bond', 'SO', 'Bond', 'PAQUETE', true),
                ('066', 'Cartulina', 'SO', 'Cartulina', 'UNIDAD', true),
                ('067', 'Marcador permanente', 'SO', 'Permanente', 'UNIDAD', true),
                ('068', 'Marcador tiza líquida', 'SO', 'Marcador', 'UNIDAD', true),
                ('069', 'Adhesivos flecha', 'SO', 'Adhesivos', 'PAQUETE', true),
                ('070', 'Pad mouse', 'SO', 'Pad mouse', 'UNIDAD', true),
                ('071', 'Papelera metálica', 'SO', 'Metálica', 'UNIDAD', true),
                ('072', 'Goma en barra', 'SO', 'Barra', 'UNIDAD', true),
                ('073', 'Pila batería', 'SO', 'Pila', 'UNIDAD', true),
                ('074', 'Pila AA', 'SO', 'Pila AA', 'UNIDAD', true),
                ('075', 'Pila AAA', 'SO', 'Pila AAA', 'UNIDAD', true),
                ('076', 'Caja de pinturas', 'SO', 'Pinturas', 'CAJA', true),
                ('077', 'Pliego papel periódico', 'SO', 'Papel periódico', 'PLIEGO', true),
                ('078', 'Rollo papel térmico 57mm*29mt', 'SO', 'Papel térmico', 'ROLLO', true),
                ('079', 'Rollo papel térmico 79mm*60mt', 'SO', 'Papel térmico', 'ROLLO', true),
                ('080', 'Separador hojas 12 colores', 'SO', 'Separador', 'PAQUETE', true),
                ('081', 'Mochila institucional', 'SO', 'Mochila', 'UNIDAD', true),
                ('082', 'Aguja costalera', 'SO', 'Costalera', 'UNIDAD', true),
                ('083', 'Caja tachuela', 'SO', 'Tachuela', 'CAJA', true),
                ('084', 'Pizarra de corcho', 'SO', 'Corcho', 'UNIDAD', true),
                ('085', 'Sello fechador', 'SO', 'Sello', 'UNIDAD', true),
                ('086', 'Hilo cometa', 'SO', 'Hilo', 'UNIDAD', true),
                ('087', 'Cartón prensado A4', 'SO', 'Cartón', 'UNIDAD', true),
                ('088', 'Sello numerador', 'SO', 'Sello numerador', 'UNIDAD', true),
                ('089', 'Portaclips', 'SO', 'Portaclips', 'CAJA', true),
                ('090', 'Separador hojas', 'SO', 'Separador', 'UNIDAD', true),
                ('091', 'Portapapelera malla', 'SO', 'Malla', 'UNIDAD', true),
                ('092', 'Tinta Epson 544 negro', 'SO', 'Epson', 'UNIDAD', true),
                ('093', 'Tinta Epson 544 cyan', 'SO', 'Epson', 'UNIDAD', true),
                ('094', 'Tinta Epson 544 yellow', 'SO', 'Epson', 'UNIDAD', true),
                ('095', 'Tinta Epson 544 magenta', 'SO', 'Epson', 'UNIDAD', true),
                ('096', 'Refuerzo hoja de papel', 'SO', 'Refuerzo', 'CAJA', true),
                ('097', 'Solicitud de crédito', 'SO', 'Solicitud', 'MILLAR', true),
                ('098', 'Informes de crédito', 'SO', 'Informes', 'MILLAR', true),
                ('099', 'Hojas para póliza', 'SO', 'Pólizas', 'MILLAR', true),
                ('100', 'Pasta A4 anillado INEN plástica', 'SO', 'INEN', 'UNIDAD', true),
                ('101', 'Crema billetes', 'SO', 'Sin marca', 'UNIDAD', true),
                ('102', 'Marcador tiza líquida', 'SO', 'Sin marca', 'UNIDAD', true),
                ('103', 'Trapeador normal', 'UAL', 'Sin marca', 'UNIDAD', true),
                ('104', 'Protector hojas', 'SO', 'Sin marca', 'UNIDAD', true),
                ('105', 'Jugos para kits', 'OI', 'Sin marca', 'UNIDAD', true),
                ('106', 'Galletas para kits', 'OI', 'Sin marca', 'UNIDAD', true),
                ('107', 'Tinta para sellos roja', 'SO', 'Lancer', 'UNIDAD', true),
                ('108', 'Cartones archivadores', 'SO', 'SN', 'UNIDAD', true),
                ('109', 'Tinta para marcador azul', 'SO', 'Edding', 'UNIDAD', true),
                ('200', 'Tinta para marcador rojo', 'SO', 'Edding', 'UNIDAD', true),
                ('201', 'Tinta para marcador negro', 'SO', 'Edding', 'UNIDAD', true),
                ('202', 'Caja de grapas', 'SO', 'Alex', 'UNIDAD', true),
                ('203', 'Galón líquido para el piso', 'UAL', 'PCV', 'LITROS', true),
                ('204', 'Palo de escoba', 'UAL', 'Sin marca', 'UNIDAD', true);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_articulo_tipo_articulo_codigo_tipo_articulo",
                schema: "proveeduria",
                table: "articulo");

            migrationBuilder.DropTable(
                name: "bodega_articulo_movimiento",
                schema: "proveeduria");

            migrationBuilder.DropTable(
                name: "solicitud_pedido_articulo",
                schema: "proveeduria");

            migrationBuilder.DropTable(
                name: "solicitud_pedido_etapa",
                schema: "proveeduria");

            migrationBuilder.DropTable(
                name: "tipo_articulo",
                schema: "proveeduria");

            migrationBuilder.DropTable(
                name: "bodega_articulo",
                schema: "proveeduria");

            migrationBuilder.DropTable(
                name: "solicitud_pedido",
                schema: "proveeduria");

            migrationBuilder.DropTable(
                name: "bodega",
                schema: "proveeduria");

            migrationBuilder.DropPrimaryKey(
                name: "pk_articulo",
                schema: "proveeduria",
                table: "articulo");

            migrationBuilder.DropIndex(
                name: "ix_articulo_codigo_tipo_articulo",
                schema: "proveeduria",
                table: "articulo");

            migrationBuilder.DropColumn(
                name: "codigo_tipo_articulo",
                schema: "proveeduria",
                table: "articulo");

            migrationBuilder.DropColumn(
                name: "multiplo",
                schema: "proveeduria",
                table: "articulo");

            migrationBuilder.AlterColumn<string>(
                name: "marca",
                schema: "proveeduria",
                table: "articulo",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "id",
                schema: "proveeduria",
                table: "articulo",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "pk_articulo",
                schema: "proveeduria",
                table: "articulo",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "ix_articulo_codigo",
                schema: "proveeduria",
                table: "articulo",
                column: "codigo",
                unique: true);
        }
    }
}

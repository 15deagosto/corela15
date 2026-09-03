using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Portafolio_MotorReal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_inversion_portafolio_codigo",
                schema: "portafolio",
                table: "inversion_portafolio");

            migrationBuilder.DropColumn(
                name: "codigo",
                schema: "portafolio",
                table: "inversion_portafolio");

            migrationBuilder.DropColumn(
                name: "institucion",
                schema: "portafolio",
                table: "inversion_portafolio");

            migrationBuilder.RenameColumn(
                name: "monto",
                schema: "portafolio",
                table: "inversion_portafolio",
                newName: "valor_nominal");

            migrationBuilder.RenameColumn(
                name: "fecha_inversion",
                schema: "portafolio",
                table: "inversion_portafolio",
                newName: "fecha_compra");

            migrationBuilder.AddColumn<string>(
                name: "codigo_institucion",
                schema: "portafolio",
                table: "inversion_portafolio",
                type: "character varying(10)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "creado_en",
                schema: "portafolio",
                table: "inversion_portafolio",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "creado_por",
                schema: "portafolio",
                table: "inversion_portafolio",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "documento",
                schema: "portafolio",
                table: "inversion_portafolio",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "modificado_en",
                schema: "portafolio",
                table: "inversion_portafolio",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "modificado_por",
                schema: "portafolio",
                table: "inversion_portafolio",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "portafolio",
                table: "inversion_portafolio",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.CreateTable(
                name: "inversion_renovacion",
                schema: "portafolio",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_inversion_origen = table.Column<Guid>(type: "uuid", nullable: false),
                    id_inversion_destino = table.Column<Guid>(type: "uuid", nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inversion_renovacion", x => x.id);
                    table.ForeignKey(
                        name: "fk_inversion_renovacion_inversion_portafolio_id_inversion_dest",
                        column: x => x.id_inversion_destino,
                        principalSchema: "portafolio",
                        principalTable: "inversion_portafolio",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_inversion_renovacion_inversion_portafolio_id_inversion_orig",
                        column: x => x.id_inversion_origen,
                        principalSchema: "portafolio",
                        principalTable: "inversion_portafolio",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tipo_institucion",
                schema: "portafolio",
                columns: table => new
                {
                    codigo = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tipo_institucion", x => x.codigo);
                });

            migrationBuilder.CreateTable(
                name: "institucion",
                schema: "portafolio",
                columns: table => new
                {
                    codigo = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    codigo_tipo_institucion = table.Column<string>(type: "character varying(10)", nullable: false),
                    activa = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_institucion", x => x.codigo);
                    table.ForeignKey(
                        name: "fk_institucion_tipo_institucion_codigo_tipo_institucion",
                        column: x => x.codigo_tipo_institucion,
                        principalSchema: "portafolio",
                        principalTable: "tipo_institucion",
                        principalColumn: "codigo",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_inversion_portafolio_codigo_institucion",
                schema: "portafolio",
                table: "inversion_portafolio",
                column: "codigo_institucion");

            migrationBuilder.CreateIndex(
                name: "ix_inversion_portafolio_documento",
                schema: "portafolio",
                table: "inversion_portafolio",
                column: "documento",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_institucion_codigo_tipo_institucion",
                schema: "portafolio",
                table: "institucion",
                column: "codigo_tipo_institucion");

            migrationBuilder.CreateIndex(
                name: "ix_inversion_renovacion_id_inversion_destino",
                schema: "portafolio",
                table: "inversion_renovacion",
                column: "id_inversion_destino",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_inversion_renovacion_id_inversion_origen",
                schema: "portafolio",
                table: "inversion_renovacion",
                column: "id_inversion_origen");

            migrationBuilder.AddForeignKey(
                name: "fk_inversion_portafolio_institucion_codigo_institucion",
                schema: "portafolio",
                table: "inversion_portafolio",
                column: "codigo_institucion",
                principalSchema: "portafolio",
                principalTable: "institucion",
                principalColumn: "codigo",
                onDelete: ReferentialAction.Restrict);

            SeedDatos(migrationBuilder);
        }

        private static void SeedDatos(MigrationBuilder migrationBuilder)
        {
            // PORTAFOLIO.TIPOINSTITUCION real (6 filas).
            migrationBuilder.Sql("""
                INSERT INTO portafolio.tipo_institucion (codigo, nombre, activo) VALUES
                ('001', 'Bancos privados', true),
                ('002', 'Bancos públicos', true),
                ('003', 'Cooperativas de ahorro y crédito', true),
                ('004', 'Mutualistas', true),
                ('005', 'Sociedades', true),
                ('006', 'Cajas centrales', true);
                """);

            // PORTAFOLIO.INSTITUCION real (44 filas — las instituciones
            // reales donde esta cooperativa ha colocado inversiones,
            // verificado contra INVERSIONPORTAFOLIO.CODIGOINSTITUCION).
            // Nombres con salto de línea embebido en la fuente real
            // (anomalía de captura de datos de Softbank) limpiados acá.
            migrationBuilder.Sql("""
                INSERT INTO portafolio.institucion (codigo, nombre, codigo_tipo_institucion, activa) VALUES
                ('001', 'Cooperativa Policía Nacional', '003', true),
                ('002', 'Cooperativa CACPECO', '003', true),
                ('003', 'Cooperativa Riobamba', '003', true),
                ('004', 'Cooperativa Atuntaqui', '003', true),
                ('005', 'Cooperativa Andalucía', '003', true),
                ('006', 'Cooperativa Tulcán', '003', true),
                ('007', 'FINANCOOP', '006', true),
                ('008', 'Cooperativa San Francisco', '003', true),
                ('009', 'Cooperativa El Sagrario', '003', true),
                ('010', 'Cooperativa Alianza del Valle', '003', true),
                ('011', 'Cooperativa Pablo Muñoz Vega', '003', true),
                ('012', 'Cooperativa de Ahorro y Crédito JEP', '003', true),
                ('013', 'Banco del Austro', '001', true),
                ('014', 'Banco Solidario', '001', true),
                ('015', 'Banco Internacional', '001', true),
                ('016', 'Cooperativa de Ahorro y Crédito Chibuleo Ltda.', '003', true),
                ('018', 'Cooperativa de Ahorro y Crédito Ambato', '003', true),
                ('019', 'Coop. Mushuc Runa Ltda.', '003', true),
                ('020', 'Banco Desarrollo de los Pueblos', '001', true),
                ('021', 'Cooperativa 9 de Octubre', '003', true),
                ('022', 'Cooperativa Fernando Daquilema', '003', true),
                ('023', 'Cooperativa Manantial de Oro Ltda.', '003', true),
                ('024', 'COAC Virgen del Cisne', '003', true),
                ('025', 'Coop. de Ahorro y Crédito Fernando Daquilema Ltda.', '003', true),
                ('026', 'Coop. de Ahorro y Crédito Once de Junio Ltda.', '003', true),
                ('027', 'Coop. de Ahorro y Crédito Santa Rosa Ltda.', '003', true),
                ('028', 'FINCA', '001', true),
                ('029', 'Coop. de Ahorro y Crédito Calceta Ltda.', '003', true),
                ('030', 'Coop. de Ahorro y Crédito Chone Ltda.', '003', true),
                ('031', 'Coop. de Ahorro y Crédito 23 de Julio Ltda.', '003', true),
                ('032', 'Coop. de Ahorro y Crédito Cooprogreso Ltda.', '003', true),
                ('033', 'Coop. de Ahorro y Crédito Alianza Minas', '003', true),
                ('034', 'Coop. de Ahorro y Crédito Policía Nacional Ltda.', '003', false),
                ('035', 'Coop. de Ahorro y Crédito Oscus Ltda.', '003', true),
                ('036', 'Coop. de Ahorro y Crédito San Francisco Ltda.', '003', false),
                ('037', 'Coop. de Ahorro y Crédito Ambato Ltda.', '003', false),
                ('038', 'Banco de Guayaquil', '001', true),
                ('039', 'Banco Pichincha', '001', true),
                ('040', 'Cooperativa 14 de Marzo', '003', true),
                ('041', 'Cooperativa Vencedores Ltda.', '003', true),
                ('042', 'Banco Pacífico', '001', true),
                ('043', 'COAC Credi Ya', '003', true),
                ('20', 'Finanzas Corporativas Ltda.', '003', true),
                ('BP01', 'Banco Produbanco', '001', true);
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_inversion_portafolio_institucion_codigo_institucion",
                schema: "portafolio",
                table: "inversion_portafolio");

            migrationBuilder.DropTable(
                name: "institucion",
                schema: "portafolio");

            migrationBuilder.DropTable(
                name: "inversion_renovacion",
                schema: "portafolio");

            migrationBuilder.DropTable(
                name: "tipo_institucion",
                schema: "portafolio");

            migrationBuilder.DropIndex(
                name: "ix_inversion_portafolio_codigo_institucion",
                schema: "portafolio",
                table: "inversion_portafolio");

            migrationBuilder.DropIndex(
                name: "ix_inversion_portafolio_documento",
                schema: "portafolio",
                table: "inversion_portafolio");

            migrationBuilder.DropColumn(
                name: "codigo_institucion",
                schema: "portafolio",
                table: "inversion_portafolio");

            migrationBuilder.DropColumn(
                name: "creado_en",
                schema: "portafolio",
                table: "inversion_portafolio");

            migrationBuilder.DropColumn(
                name: "creado_por",
                schema: "portafolio",
                table: "inversion_portafolio");

            migrationBuilder.DropColumn(
                name: "documento",
                schema: "portafolio",
                table: "inversion_portafolio");

            migrationBuilder.DropColumn(
                name: "modificado_en",
                schema: "portafolio",
                table: "inversion_portafolio");

            migrationBuilder.DropColumn(
                name: "modificado_por",
                schema: "portafolio",
                table: "inversion_portafolio");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "portafolio",
                table: "inversion_portafolio");

            migrationBuilder.RenameColumn(
                name: "valor_nominal",
                schema: "portafolio",
                table: "inversion_portafolio",
                newName: "monto");

            migrationBuilder.RenameColumn(
                name: "fecha_compra",
                schema: "portafolio",
                table: "inversion_portafolio",
                newName: "fecha_inversion");

            migrationBuilder.AddColumn<string>(
                name: "codigo",
                schema: "portafolio",
                table: "inversion_portafolio",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "institucion",
                schema: "portafolio",
                table: "inversion_portafolio",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "ix_inversion_portafolio_codigo",
                schema: "portafolio",
                table: "inversion_portafolio",
                column: "codigo",
                unique: true);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Sujeto_CatalogosSocioeconomicos : Migration
    {
        // 212 filas reales — verificado contra SUJETO.NACIONALIDAD (Softbank,
        // solo lectura), lista ISO de países, solo las filas con ACTIVO=1.
        private static readonly string[] NacionalidadSeed =
        {
            "('ABW', 'Aruba', true)",
            "('AFG', 'Afganistán', true)",
            "('AGO', 'Angola', true)",
            "('AIA', 'Anguila', true)",
            "('ALA', 'Åland', true)",
            "('ALB', 'Albania', true)",
            "('AND', 'Andorra', true)",
            "('ANT', 'Antillas Neerlandesas', true)",
            "('ARE', 'Emiratos Árabes Unidos', true)",
            "('ARG', 'Argentina', true)",
            "('ARM', 'Armenia', true)",
            "('ASM', 'Samoa Americana', true)",
            "('ATA', 'Antártida', true)",
            "('ATF', 'Territorios Australes Franceses', true)",
            "('ATG', 'Antigua y Barbuda', true)",
            "('AUS', 'Australia', true)",
            "('AUT', 'Austria', true)",
            "('AZE', 'Azerbaiyán', true)",
            "('BDI', 'Burundi', true)",
            "('BEL', 'Bélgica', true)",
            "('BEN', 'Benín', true)",
            "('BFA', 'Burkina Faso', true)",
            "('BGD', 'Bangladesh', true)",
            "('BGR', 'Bulgaria', true)",
            "('BHR', 'Bahréin', true)",
            "('BHS', 'Bahamas', true)",
            "('BIH', 'Bosnia y Herzegovina', true)",
            "('BLM', 'San Bartolomé', true)",
            "('BLR', 'Bielorrusia', true)",
            "('BLZ', 'Belice', true)",
            "('BMU', 'Bermudas', true)",
            "('BOL', 'Bolivia', true)",
            "('BRA', 'Brasil', true)",
            "('BRB', 'Barbados', true)",
            "('BRN', 'Brunéi', true)",
            "('BTN', 'Bután', true)",
            "('BVT', 'Isla Bouvet', true)",
            "('BWA', 'Botsuana', true)",
            "('CAF', 'República Centroafricana', true)",
            "('CAN', 'Canadá', true)",
            "('CCK', 'Islas Cocos', true)",
            "('CHE', 'Suiza', true)",
            "('CHL', 'Chile', true)",
            "('CHN', 'China', true)",
            "('CIV', 'Costa de Marfil', true)",
            "('CMR', 'Camerún', true)",
            "('COD', 'República Democrática del Congo', true)",
            "('COG', 'República del Congo', true)",
            "('COK', 'Islas Cook', true)",
            "('COL', 'Colombia', true)",
            "('COM', 'Comoras', true)",
            "('CPV', 'Cabo Verde', true)",
            "('CRI', 'Costa Rica', true)",
            "('CUB', 'Cuba', true)",
            "('CYM', 'Islas Caimán', true)",
            "('CYP', 'Chipre', true)",
            "('CZE', 'República Checa', true)",
            "('DEU', 'Alemania', true)",
            "('DJI', 'Yibuti', true)",
            "('DMA', 'Dominica', true)",
            "('DNK', 'Dinamarca', true)",
            "('DOM', 'República Dominicana', true)",
            "('DZA', 'Argelia', true)",
            "('ECU', 'Ecuador', true)",
            "('EGY', 'Egipto', true)",
            "('ERI', 'Eritrea', true)",
            "('ESH', 'Sahara Occidental', true)",
            "('ESP', 'España', true)",
            "('EST', 'Estonia', true)",
            "('ETH', 'Etiopía', true)",
            "('EUR', 'Unión Europea', true)",
            "('FIN', 'Finlandia', true)",
            "('FJI', 'Fiyi', true)",
            "('FLK', 'Islas Malvinas', true)",
            "('FRA', 'Francia', true)",
            "('FRO', 'Islas Feroe', true)",
            "('FSM', 'Micronesia', true)",
            "('GAB', 'Gabón', true)",
            "('GEO', 'Georgia', true)",
            "('GGY', 'Guernsey', true)",
            "('GHA', 'Ghana', true)",
            "('GIB', 'Gibraltar', true)",
            "('GIN', 'Guinea', true)",
            "('GLP', 'Guadalupe', true)",
            "('GMB', 'Gambia', true)",
            "('GNB', 'Guinea-Bissau', true)",
            "('GNQ', 'Guinea Ecuatorial', true)",
            "('GRC', 'Grecia', true)",
            "('GRD', 'Granada', true)",
            "('GRL', 'Groenlandia', true)",
            "('GTM', 'Guatemala', true)",
            "('GUF', 'Guayana Francesa', true)",
            "('GUM', 'Guam', true)",
            "('GUY', 'Guyana', true)",
            "('HKG', 'Hong Kong', true)",
            "('HMD', 'Islas Heard y McDonald', true)",
            "('HND', 'Honduras', true)",
            "('HRV', 'Croacia', true)",
            "('HTI', 'Haití', true)",
            "('HUN', 'Hungría', true)",
            "('IDN', 'Indonesia', true)",
            "('IMN', 'Isla de Man', true)",
            "('IND', 'India', true)",
            "('IOT', 'Territorio Británico del Océano Índico', true)",
            "('IRL', 'Irlanda', true)",
            "('IRN', 'Irán', true)",
            "('IRQ', 'Iraq', true)",
            "('ISL', 'Islandia', true)",
            "('ISR', 'Israel', true)",
            "('ITA', 'Italia', true)",
            "('JAM', 'Jamaica', true)",
            "('JEY', 'Jersey', true)",
            "('JOR', 'Jordania', true)",
            "('JPN', 'Japón', true)",
            "('KAZ', 'Kazajistán', true)",
            "('KEN', 'Kenia', true)",
            "('KGZ', 'Kirguistán', true)",
            "('KHM', 'Camboya', true)",
            "('KIR', 'Kiribati', true)",
            "('KNA', 'San Cristóbal y Nieves', true)",
            "('KOR', 'Corea del Sur', true)",
            "('KWT', 'Kuwait', true)",
            "('LAO', 'Laos', true)",
            "('LBN', 'Líbano', true)",
            "('LBR', 'Liberia', true)",
            "('LBY', 'Libia', true)",
            "('LCA', 'Santa Lucía', true)",
            "('LIE', 'Liechtenstein', true)",
            "('LKA', 'Sri Lanka', true)",
            "('LSO', 'Lesoto', true)",
            "('LTU', 'Lituania', true)",
            "('LUX', 'Luxemburgo', true)",
            "('LVA', 'Letonia', true)",
            "('MAC', 'Macao', true)",
            "('MAR', 'Marruecos', true)",
            "('MCO', 'Mónaco', true)",
            "('MDA', 'Moldavia', true)",
            "('MDG', 'Madagascar', true)",
            "('MDV', 'Maldivas', true)",
            "('MEX', 'México', true)",
            "('MHL', 'Islas Marshall', true)",
            "('MKD', 'Macedonia', true)",
            "('MLI', 'Malí', true)",
            "('MLT', 'Malta', true)",
            "('MMR', 'Birmania', true)",
            "('MNP', 'Islas Marianas del Norte', true)",
            "('MRT', 'Mauritania', true)",
            "('MTQ', 'Martinica', true)",
            "('MUS', 'Mauricio', true)",
            "('MWI', 'Malaui', true)",
            "('MYS', 'Malasia', true)",
            "('MYT', 'Mayotte', true)",
            "('PHL', 'Filipinas', true)",
            "('PRK', 'Corea del Norte', true)",
            "('RUS', 'Rusia', true)",
            "('SAU', 'Arabia Saudita', true)",
            "('SDN', 'Sudán', true)",
            "('SEN', 'Senegal', true)",
            "('SGP', 'Singapur', true)",
            "('SGS', 'Islas Georgias del Sur y Sandwich del Sur', true)",
            "('SHN', 'Santa Helena', true)",
            "('SJM', 'Svalbard y Jan Mayen', true)",
            "('SLB', 'Islas Salomón', true)",
            "('SLE', 'Sierra Leona', true)",
            "('SLV', 'El Salvador', true)",
            "('SMR', 'San Marino', true)",
            "('SOM', 'Somalia', true)",
            "('SPM', 'San Pedro y Miquelón', true)",
            "('SRB', 'Serbia', true)",
            "('STP', 'Santo Tomé y Príncipe', true)",
            "('SUR', 'Surinam', true)",
            "('SVK', 'Eslovaquia', true)",
            "('SVN', 'Eslovenia', true)",
            "('SWE', 'Suecia', true)",
            "('SWZ', 'Suazilandia', true)",
            "('SYC', 'Seychelles', true)",
            "('SYR', 'Siria', true)",
            "('TCA', 'Islas Turcas y Caicos', true)",
            "('TCD', 'Chad', true)",
            "('TGO', 'Togo', true)",
            "('THA', 'Tailandia', true)",
            "('TJK', 'Tayikistán', true)",
            "('TKL', 'Tokelau', true)",
            "('TKM', 'Turkmenistán', true)",
            "('TLS', 'Timor Oriental', true)",
            "('TON', 'Tonga', true)",
            "('TTO', 'Trinidad y Tobago', true)",
            "('TUN', 'Túnez', true)",
            "('TUR', 'Turquía', true)",
            "('TUV', 'Tuvalu', true)",
            "('TWN', 'Taiwán', true)",
            "('TZA', 'Tanzania', true)",
            "('UGA', 'Uganda', true)",
            "('UKR', 'Ucrania', true)",
            "('UMI', 'Islas ultramarinas de Estados Unidos', true)",
            "('URS', 'Unión Soviética', true)",
            "('URY', 'Uruguay', true)",
            "('USA', 'Estados Unidos', true)",
            "('UZB', 'Uzbekistán', true)",
            "('VAT', 'Ciudad del Vaticano', true)",
            "('VCT', 'San Vicente y las Granadinas', true)",
            "('VEN', 'Venezuela', true)",
            "('VGB', 'Islas Vírgenes Británicas', true)",
            "('VIR', 'Islas Vírgenes Estadounidenses', true)",
            "('VNM', 'Vietnam', true)",
            "('VUT', 'Vanuatu', true)",
            "('WLF', 'Wallis y Futuna', true)",
            "('WSM', 'Samoa', true)",
            "('YEM', 'Yemen', true)",
            "('ZAF', 'Sudáfrica', true)",
            "('ZMB', 'Zambia', true)",
            "('ZWE', 'Zimbabue', true)",
        };

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "codigo_educacion",
                schema: "sujeto",
                table: "persona_natural",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "codigo_estado_civil",
                schema: "sujeto",
                table: "persona_natural",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "codigo_nacionalidad",
                schema: "sujeto",
                table: "persona_natural",
                type: "character varying(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "codigo_sector_vivienda",
                schema: "sujeto",
                table: "persona_natural",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "codigo_vivienda",
                schema: "sujeto",
                table: "persona_natural",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "codigo_calificacion_interna",
                schema: "clientes",
                table: "cliente",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "codigo_causa_vinculacion",
                schema: "clientes",
                table: "cliente",
                type: "character varying(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "codigo_sector_economico",
                schema: "clientes",
                table: "cliente",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "calificacion_interna",
                schema: "clientes",
                columns: table => new
                {
                    codigo = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    activa = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_calificacion_interna", x => x.codigo);
                });

            migrationBuilder.CreateTable(
                name: "causa_vinculacion",
                schema: "clientes",
                columns: table => new
                {
                    codigo = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(600)", maxLength: 600, nullable: false),
                    activa = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_causa_vinculacion", x => x.codigo);
                });

            migrationBuilder.CreateTable(
                name: "educacion",
                schema: "sujeto",
                columns: table => new
                {
                    codigo = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_educacion", x => x.codigo);
                });

            migrationBuilder.CreateTable(
                name: "estado_civil",
                schema: "sujeto",
                columns: table => new
                {
                    codigo = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_estado_civil", x => x.codigo);
                });

            migrationBuilder.CreateTable(
                name: "nacionalidad",
                schema: "sujeto",
                columns: table => new
                {
                    codigo = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_nacionalidad", x => x.codigo);
                });

            migrationBuilder.CreateTable(
                name: "sector_economico",
                schema: "clientes",
                columns: table => new
                {
                    codigo = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    activa = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sector_economico", x => x.codigo);
                });

            migrationBuilder.CreateTable(
                name: "sector_vivienda",
                schema: "sujeto",
                columns: table => new
                {
                    codigo = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sector_vivienda", x => x.codigo);
                });

            migrationBuilder.CreateTable(
                name: "vivienda",
                schema: "sujeto",
                columns: table => new
                {
                    codigo = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vivienda", x => x.codigo);
                });

            // Datos reales verificados contra Softbank (solo lectura) — ver
            // CLAUDE.md sección "CRUD real de Socios y Usuarios y roles" para
            // el detalle de la verificación fila por fila.
            migrationBuilder.Sql(@"
INSERT INTO sujeto.estado_civil (codigo, nombre, activo) VALUES
('C', 'CASADO', true),
('D', 'DIVORCIADO', true),
('P', 'SEPARACION DE BIENES', true),
('S', 'SOLTERO', true),
('U', 'UNION LIBRE', true),
('V', 'VIUDO/A', true),
('Z', 'SEPARADO', false);

INSERT INTO sujeto.educacion (codigo, nombre, activo) VALUES
('A', 'ANALFABETO', false),
('BS', 'BASICA', false),
('CU', 'CUARTO NIVEL', false),
('EC', 'ECONOMISTA', false),
('G', 'POSTGRADO', true),
('H', 'PHD', false),
('I', 'INICIAL', false),
('L', 'LICENCIATURA', false),
('M', 'MAESTRIA', false),
('N', 'NINGUNA', true),
('P', 'PRIMARIA', true),
('S', 'BACHILLERATO', true),
('T', 'INTERMEDIA / TECNICO', true),
('U', 'UNIVERSITARIA', true),
('V', 'TECNOLOGO', false);

INSERT INTO sujeto.vivienda (codigo, nombre, activo) VALUES
('A', 'ARRIENDO', true),
('AN', 'ANTICRESIS', false),
('C', 'CONJUNTA', false),
('E', 'NO ESPECIFICADA', false),
('F', 'FAMILIAR', true),
('H', 'HIPOTECADA', false),
('N', 'NO INFORMA', false),
('O', 'OTROS', false),
('P', 'PROPIA', true),
('S', 'NO CLASIFICADA', false),
('T', 'PRESTADA', false),
('X', 'ACTUALIZAR', false),
('Z', 'ADOSADA', false);

INSERT INTO sujeto.sector_vivienda (codigo, nombre, activo) VALUES
('O', 'OTROS', false),
('R', 'RURAL', true),
('SN', 'SIN ASIGNAR', false),
('U', 'URBANA', true),
('UR', 'PERIURBANA', false);

INSERT INTO clientes.causa_vinculacion (codigo, descripcion, activa) VALUES
('A01', 'Las personas juridicas en las cuales los administradores o funcionarios que aprueban operaciones de credito de una entidad financiera posean directa o indirectamente mas del 3% del capital de dichas sociedades.', true),
('A02', 'Las personas juridicas en las que los conyuges, los convivientes, los parientes dentro del segundo grado de consanguinidad o primero de afinidad de los administradores o de los funcionarios que aprueban operaciones de credito de una entidad financiera, posean acciones por un 3% o mas del capital de dichas sociedades.', true),
('A03', 'Operaciones que superen los cupos de credito establecidos en el Codigo Organico Monetario y Financiero y en la normativa que expida la Junta de Politica y Regulacion Monetaria y Financiera.', true),
('E01', 'Las que hayan recibido creditos en condiciones preferenciales por plazos, tasas de interes, falta de caucion o desproporcionadas respecto del patrimonio del deudor o de su capacidad de pago.', true),
('E02', 'Las que hayan recibido creditos no garantizados adecuadamente, sin antecedentes o domiciliados en el extranjero y sin informacion disponible sobre ellos.', true),
('E03', 'Las que hayan recibido creditos por reciprocidad con otra entidad financiera.', true),
('E04', 'Las que tengan tratamientos preferenciales en operaciones pasivas', true),
('E05', 'Las que se declaren presuntivas, con arreglo a las normas de caracter general dictadas por los organismos de control', true),
('G01', 'Sociedad en la que los administradores directos o funcionarios de la institucion controlada sean titulares directa o indirectamente de mas del 3% del capital pagado de dicha sociedad', true),
('G02', 'Conyuges o parientes dentro del cuarto grado de consanguinidad o segundo de afinidad de los administradores directos o funcionarios de la institucion controlada', true),
('G03', 'Los administradores directos o funcionarios de una institucion controlada', true),
('G04', 'TRATO PREFERENCIAL', true),
('G05', 'Sociedad que tenga entre sus socios o accionistas a otras sociedades, cuando estas sean titulares del 10% o mas del capital pagado de la primera; y, adicionalmente figuren entre sus accionistas o socios, con el 3% o mas del capital pagado, administradores comunes.', true),
('G06', 'Sociedad cuyos directores, principales o suplentes, o los representantes legales o los apoderados generales, sean tambien administradores directos o funcionarios de una institucion controlada.', true),
('NV', 'No vinculado', true);

INSERT INTO clientes.calificacion_interna (codigo, nombre, activa) VALUES
('B', 'Buena', false),
('N', 'No Aplica', true),
('R', 'Regular', false);

INSERT INTO clientes.sector_economico (codigo, nombre, activa) VALUES
('A', 'Alto', true),
('B', 'Bajo', true),
('M', 'Medio', true),
('MA', 'Medio Alto', false),
('MB', 'Medio Bajo', false),
('NI', 'No Aplica', false);

INSERT INTO sujeto.nacionalidad (codigo, nombre, activo) VALUES
" + string.Join(",\n", NacionalidadSeed) + @";
");

            migrationBuilder.CreateIndex(
                name: "ix_persona_natural_codigo_educacion",
                schema: "sujeto",
                table: "persona_natural",
                column: "codigo_educacion");

            migrationBuilder.CreateIndex(
                name: "ix_persona_natural_codigo_estado_civil",
                schema: "sujeto",
                table: "persona_natural",
                column: "codigo_estado_civil");

            migrationBuilder.CreateIndex(
                name: "ix_persona_natural_codigo_nacionalidad",
                schema: "sujeto",
                table: "persona_natural",
                column: "codigo_nacionalidad");

            migrationBuilder.CreateIndex(
                name: "ix_persona_natural_codigo_sector_vivienda",
                schema: "sujeto",
                table: "persona_natural",
                column: "codigo_sector_vivienda");

            migrationBuilder.CreateIndex(
                name: "ix_persona_natural_codigo_vivienda",
                schema: "sujeto",
                table: "persona_natural",
                column: "codigo_vivienda");

            migrationBuilder.CreateIndex(
                name: "ix_cliente_codigo_calificacion_interna",
                schema: "clientes",
                table: "cliente",
                column: "codigo_calificacion_interna");

            migrationBuilder.CreateIndex(
                name: "ix_cliente_codigo_causa_vinculacion",
                schema: "clientes",
                table: "cliente",
                column: "codigo_causa_vinculacion");

            migrationBuilder.CreateIndex(
                name: "ix_cliente_codigo_sector_economico",
                schema: "clientes",
                table: "cliente",
                column: "codigo_sector_economico");

            migrationBuilder.AddForeignKey(
                name: "fk_cliente_calificacion_interna_codigo_calificacion_interna",
                schema: "clientes",
                table: "cliente",
                column: "codigo_calificacion_interna",
                principalSchema: "clientes",
                principalTable: "calificacion_interna",
                principalColumn: "codigo",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_cliente_causa_vinculacion_codigo_causa_vinculacion",
                schema: "clientes",
                table: "cliente",
                column: "codigo_causa_vinculacion",
                principalSchema: "clientes",
                principalTable: "causa_vinculacion",
                principalColumn: "codigo",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_cliente_sector_economico_codigo_sector_economico",
                schema: "clientes",
                table: "cliente",
                column: "codigo_sector_economico",
                principalSchema: "clientes",
                principalTable: "sector_economico",
                principalColumn: "codigo",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_persona_natural_educacion_codigo_educacion",
                schema: "sujeto",
                table: "persona_natural",
                column: "codigo_educacion",
                principalSchema: "sujeto",
                principalTable: "educacion",
                principalColumn: "codigo",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_persona_natural_estado_civil_codigo_estado_civil",
                schema: "sujeto",
                table: "persona_natural",
                column: "codigo_estado_civil",
                principalSchema: "sujeto",
                principalTable: "estado_civil",
                principalColumn: "codigo",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_persona_natural_nacionalidad_codigo_nacionalidad",
                schema: "sujeto",
                table: "persona_natural",
                column: "codigo_nacionalidad",
                principalSchema: "sujeto",
                principalTable: "nacionalidad",
                principalColumn: "codigo",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_persona_natural_sector_vivienda_codigo_sector_vivienda",
                schema: "sujeto",
                table: "persona_natural",
                column: "codigo_sector_vivienda",
                principalSchema: "sujeto",
                principalTable: "sector_vivienda",
                principalColumn: "codigo",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_persona_natural_vivienda_codigo_vivienda",
                schema: "sujeto",
                table: "persona_natural",
                column: "codigo_vivienda",
                principalSchema: "sujeto",
                principalTable: "vivienda",
                principalColumn: "codigo",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_cliente_calificacion_interna_codigo_calificacion_interna",
                schema: "clientes",
                table: "cliente");

            migrationBuilder.DropForeignKey(
                name: "fk_cliente_causa_vinculacion_codigo_causa_vinculacion",
                schema: "clientes",
                table: "cliente");

            migrationBuilder.DropForeignKey(
                name: "fk_cliente_sector_economico_codigo_sector_economico",
                schema: "clientes",
                table: "cliente");

            migrationBuilder.DropForeignKey(
                name: "fk_persona_natural_educacion_codigo_educacion",
                schema: "sujeto",
                table: "persona_natural");

            migrationBuilder.DropForeignKey(
                name: "fk_persona_natural_estado_civil_codigo_estado_civil",
                schema: "sujeto",
                table: "persona_natural");

            migrationBuilder.DropForeignKey(
                name: "fk_persona_natural_nacionalidad_codigo_nacionalidad",
                schema: "sujeto",
                table: "persona_natural");

            migrationBuilder.DropForeignKey(
                name: "fk_persona_natural_sector_vivienda_codigo_sector_vivienda",
                schema: "sujeto",
                table: "persona_natural");

            migrationBuilder.DropForeignKey(
                name: "fk_persona_natural_vivienda_codigo_vivienda",
                schema: "sujeto",
                table: "persona_natural");

            migrationBuilder.DropTable(
                name: "calificacion_interna",
                schema: "clientes");

            migrationBuilder.DropTable(
                name: "causa_vinculacion",
                schema: "clientes");

            migrationBuilder.DropTable(
                name: "educacion",
                schema: "sujeto");

            migrationBuilder.DropTable(
                name: "estado_civil",
                schema: "sujeto");

            migrationBuilder.DropTable(
                name: "nacionalidad",
                schema: "sujeto");

            migrationBuilder.DropTable(
                name: "sector_economico",
                schema: "clientes");

            migrationBuilder.DropTable(
                name: "sector_vivienda",
                schema: "sujeto");

            migrationBuilder.DropTable(
                name: "vivienda",
                schema: "sujeto");

            migrationBuilder.DropIndex(
                name: "ix_persona_natural_codigo_educacion",
                schema: "sujeto",
                table: "persona_natural");

            migrationBuilder.DropIndex(
                name: "ix_persona_natural_codigo_estado_civil",
                schema: "sujeto",
                table: "persona_natural");

            migrationBuilder.DropIndex(
                name: "ix_persona_natural_codigo_nacionalidad",
                schema: "sujeto",
                table: "persona_natural");

            migrationBuilder.DropIndex(
                name: "ix_persona_natural_codigo_sector_vivienda",
                schema: "sujeto",
                table: "persona_natural");

            migrationBuilder.DropIndex(
                name: "ix_persona_natural_codigo_vivienda",
                schema: "sujeto",
                table: "persona_natural");

            migrationBuilder.DropIndex(
                name: "ix_cliente_codigo_calificacion_interna",
                schema: "clientes",
                table: "cliente");

            migrationBuilder.DropIndex(
                name: "ix_cliente_codigo_causa_vinculacion",
                schema: "clientes",
                table: "cliente");

            migrationBuilder.DropIndex(
                name: "ix_cliente_codigo_sector_economico",
                schema: "clientes",
                table: "cliente");

            migrationBuilder.DropColumn(
                name: "codigo_educacion",
                schema: "sujeto",
                table: "persona_natural");

            migrationBuilder.DropColumn(
                name: "codigo_estado_civil",
                schema: "sujeto",
                table: "persona_natural");

            migrationBuilder.DropColumn(
                name: "codigo_nacionalidad",
                schema: "sujeto",
                table: "persona_natural");

            migrationBuilder.DropColumn(
                name: "codigo_sector_vivienda",
                schema: "sujeto",
                table: "persona_natural");

            migrationBuilder.DropColumn(
                name: "codigo_vivienda",
                schema: "sujeto",
                table: "persona_natural");

            migrationBuilder.DropColumn(
                name: "codigo_calificacion_interna",
                schema: "clientes",
                table: "cliente");

            migrationBuilder.DropColumn(
                name: "codigo_causa_vinculacion",
                schema: "clientes",
                table: "cliente");

            migrationBuilder.DropColumn(
                name: "codigo_sector_economico",
                schema: "clientes",
                table: "cliente");
        }
    }
}

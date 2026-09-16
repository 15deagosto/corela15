using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Documentos_CarpetasReales : Migration
    {
        // Mapeo real área→carpeta raíz, reusado en varios bloques SQL de
        // esta migración -- las 15 áreas reales ya levantadas por TI se
        // preservan tal cual como carpetas raíz, sin perder nada de la
        // organización existente.
        private const string MapeoAreaCarpeta = """
            WITH mapeo(area_codigo, carpeta_nombre) AS (
                VALUES
                    ('GerenciaGeneral', 'Gerencia General'),
                    ('NegociosComercial', 'Negocios / Comercial'),
                    ('Credito', 'Crédito'),
                    ('Captacion', 'Captación'),
                    ('CajasVentanilla', 'Cajas / Ventanilla'),
                    ('AtencionCliente', 'Atención al Cliente'),
                    ('Cobranzas', 'Cobranzas'),
                    ('Cumplimiento', 'Cumplimiento'),
                    ('Riesgos', 'Riesgos'),
                    ('ContabilidadFinanzas', 'Contabilidad / Finanzas'),
                    ('TalentoHumano', 'Talento Humano'),
                    ('AuditoriaInterna', 'Auditoría Interna'),
                    ('AgenciasSucursales', 'Agencias / Sucursales'),
                    ('SistemasTi', 'Sistemas / TI'),
                    ('Otra', 'Otra')
            )
            """;

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "carpeta",
                schema: "documentos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    id_carpeta_padre = table.Column<Guid>(type: "uuid", nullable: true),
                    activa = table.Column<bool>(type: "boolean", nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_carpeta", x => x.id);
                    table.ForeignKey(
                        name: "fk_carpeta_carpeta_id_carpeta_padre",
                        column: x => x.id_carpeta_padre,
                        principalSchema: "documentos",
                        principalTable: "carpeta",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "carpeta_acceso",
                schema: "documentos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_carpeta = table.Column<Guid>(type: "uuid", nullable: false),
                    id_usuario = table.Column<Guid>(type: "uuid", nullable: false),
                    nivel_acceso = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_carpeta_acceso", x => x.id);
                    table.ForeignKey(
                        name: "fk_carpeta_acceso_carpetas_id_carpeta",
                        column: x => x.id_carpeta,
                        principalSchema: "documentos",
                        principalTable: "carpeta",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_carpeta_acceso_usuarios_id_usuario",
                        column: x => x.id_usuario,
                        principalSchema: "seguridad",
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Nullable por ahora -- se completa por SQL abajo antes de
            // exigirla NOT NULL, para poder mapear cada documento real
            // existente a su carpeta raíz correspondiente.
            migrationBuilder.AddColumn<Guid>(
                name: "id_carpeta",
                schema: "documentos",
                table: "documento",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql("""
                INSERT INTO documentos.carpeta (id, nombre, id_carpeta_padre, activa, creado_en, creado_por)
                VALUES
                    (gen_random_uuid(), 'Gerencia General', NULL, true, now(), 'migracion:carpetas_reales'),
                    (gen_random_uuid(), 'Negocios / Comercial', NULL, true, now(), 'migracion:carpetas_reales'),
                    (gen_random_uuid(), 'Crédito', NULL, true, now(), 'migracion:carpetas_reales'),
                    (gen_random_uuid(), 'Captación', NULL, true, now(), 'migracion:carpetas_reales'),
                    (gen_random_uuid(), 'Cajas / Ventanilla', NULL, true, now(), 'migracion:carpetas_reales'),
                    (gen_random_uuid(), 'Atención al Cliente', NULL, true, now(), 'migracion:carpetas_reales'),
                    (gen_random_uuid(), 'Cobranzas', NULL, true, now(), 'migracion:carpetas_reales'),
                    (gen_random_uuid(), 'Cumplimiento', NULL, true, now(), 'migracion:carpetas_reales'),
                    (gen_random_uuid(), 'Riesgos', NULL, true, now(), 'migracion:carpetas_reales'),
                    (gen_random_uuid(), 'Contabilidad / Finanzas', NULL, true, now(), 'migracion:carpetas_reales'),
                    (gen_random_uuid(), 'Talento Humano', NULL, true, now(), 'migracion:carpetas_reales'),
                    (gen_random_uuid(), 'Auditoría Interna', NULL, true, now(), 'migracion:carpetas_reales'),
                    (gen_random_uuid(), 'Agencias / Sucursales', NULL, true, now(), 'migracion:carpetas_reales'),
                    (gen_random_uuid(), 'Sistemas / TI', NULL, true, now(), 'migracion:carpetas_reales'),
                    (gen_random_uuid(), 'Otra', NULL, true, now(), 'migracion:carpetas_reales');
                """);

            migrationBuilder.Sql($"""
                {MapeoAreaCarpeta}
                UPDATE documentos.documento d
                SET id_carpeta = c.id
                FROM mapeo m
                JOIN documentos.carpeta c ON c.nombre = m.carpeta_nombre AND c.id_carpeta_padre IS NULL
                WHERE d.area = m.area_codigo;
                """);

            // Cualquier documento con un código de área que no calce con el
            // mapeo (no debería pasar, el enum es cerrado) cae en "Otra"
            // -- nunca se deja un id_carpeta nulo antes del NOT NULL de abajo.
            migrationBuilder.Sql("""
                UPDATE documentos.documento
                SET id_carpeta = (SELECT id FROM documentos.carpeta WHERE nombre = 'Otra' AND id_carpeta_padre IS NULL)
                WHERE id_carpeta IS NULL;
                """);

            // El ACL real ya otorgado por área se traduce 1:1 a la carpeta
            // raíz correspondiente -- nadie pierde el acceso que ya tenía.
            migrationBuilder.Sql($"""
                {MapeoAreaCarpeta}
                INSERT INTO documentos.carpeta_acceso (id, id_carpeta, id_usuario, nivel_acceso, creado_en, creado_por)
                SELECT gen_random_uuid(), c.id, a.id_usuario, a.nivel_acceso, a.creado_en, a.creado_por
                FROM documentos.area_acceso_usuario a
                JOIN mapeo m ON m.area_codigo = a.area
                JOIN documentos.carpeta c ON c.nombre = m.carpeta_nombre AND c.id_carpeta_padre IS NULL;
                """);

            migrationBuilder.DropTable(
                name: "area_acceso_usuario",
                schema: "documentos");

            migrationBuilder.AlterColumn<Guid>(
                name: "id_carpeta",
                schema: "documentos",
                table: "documento",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_documento_id_carpeta",
                schema: "documentos",
                table: "documento",
                column: "id_carpeta");

            migrationBuilder.CreateIndex(
                name: "ix_carpeta_activa",
                schema: "documentos",
                table: "carpeta",
                column: "activa");

            migrationBuilder.CreateIndex(
                name: "ix_carpeta_id_carpeta_padre",
                schema: "documentos",
                table: "carpeta",
                column: "id_carpeta_padre");

            migrationBuilder.CreateIndex(
                name: "ix_carpeta_acceso_id_carpeta_id_usuario",
                schema: "documentos",
                table: "carpeta_acceso",
                columns: new[] { "id_carpeta", "id_usuario" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_carpeta_acceso_id_usuario",
                schema: "documentos",
                table: "carpeta_acceso",
                column: "id_usuario");

            migrationBuilder.AddForeignKey(
                name: "fk_documento_carpeta_id_carpeta",
                schema: "documentos",
                table: "documento",
                column: "id_carpeta",
                principalSchema: "documentos",
                principalTable: "carpeta",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_documento_carpeta_id_carpeta",
                schema: "documentos",
                table: "documento");

            migrationBuilder.DropTable(
                name: "carpeta_acceso",
                schema: "documentos");

            migrationBuilder.DropTable(
                name: "carpeta",
                schema: "documentos");

            migrationBuilder.DropIndex(
                name: "ix_documento_id_carpeta",
                schema: "documentos",
                table: "documento");

            migrationBuilder.DropColumn(
                name: "id_carpeta",
                schema: "documentos",
                table: "documento");

            // Nota: revierte solo el esquema -- los otorgamientos reales
            // que se hayan hecho por carpeta (no por área) en el tiempo que
            // corrió este modelo no tienen forma real de traducirse de
            // vuelta a un área única, se pierden en un rollback real.
            migrationBuilder.CreateTable(
                name: "area_acceso_usuario",
                schema: "documentos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_usuario = table.Column<Guid>(type: "uuid", nullable: false),
                    area = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    nivel_acceso = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_area_acceso_usuario", x => x.id);
                    table.ForeignKey(
                        name: "fk_area_acceso_usuario_usuarios_id_usuario",
                        column: x => x.id_usuario,
                        principalSchema: "seguridad",
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_area_acceso_usuario_id_usuario_area",
                schema: "documentos",
                table: "area_acceso_usuario",
                columns: new[] { "id_usuario", "area" },
                unique: true);
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Nivel0_Cimientos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "seguridad");

            migrationBuilder.EnsureSchema(
                name: "general");

            migrationBuilder.EnsureSchema(
                name: "clientes");

            migrationBuilder.EnsureSchema(
                name: "sujeto");

            migrationBuilder.CreateTable(
                name: "moneda",
                schema: "general",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    codigo = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    simbolo = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_moneda", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "pais",
                schema: "general",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    codigo = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pais", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rol",
                schema: "seguridad",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    nivel = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_rol", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tipo_identificacion",
                schema: "general",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    codigo = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tipo_identificacion", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "empresa",
                schema: "general",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    codigo = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ruc = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    id_moneda = table.Column<int>(type: "integer", nullable: false),
                    id_pais = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_empresa", x => x.id);
                    table.ForeignKey(
                        name: "fk_empresa_monedas_id_moneda",
                        column: x => x.id_moneda,
                        principalSchema: "general",
                        principalTable: "moneda",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_empresa_paises_id_pais",
                        column: x => x.id_pais,
                        principalSchema: "general",
                        principalTable: "pais",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "persona",
                schema: "sujeto",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    identificacion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    id_tipo_identificacion = table.Column<int>(type: "integer", nullable: false),
                    nombre = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    id_pais = table.Column<int>(type: "integer", nullable: true),
                    id_actividad_economica = table.Column<int>(type: "integer", nullable: true),
                    activos = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    pasivos = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    ingresos = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    egresos = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    numero_casa = table.Column<string>(type: "text", nullable: true),
                    barrio = table.Column<string>(type: "text", nullable: true),
                    calle_principal = table.Column<string>(type: "text", nullable: true),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    modificado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    modificado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_persona", x => x.id);
                    table.ForeignKey(
                        name: "fk_persona_pais_id_pais",
                        column: x => x.id_pais,
                        principalSchema: "general",
                        principalTable: "pais",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_persona_tipos_identificacion_id_tipo_identificacion",
                        column: x => x.id_tipo_identificacion,
                        principalSchema: "general",
                        principalTable: "tipo_identificacion",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "agencia",
                schema: "general",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_empresa = table.Column<int>(type: "integer", nullable: false),
                    codigo = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    es_operativa = table.Column<bool>(type: "boolean", nullable: false),
                    activa = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_agencia", x => x.id);
                    table.ForeignKey(
                        name: "fk_agencia_empresas_id_empresa",
                        column: x => x.id_empresa,
                        principalSchema: "general",
                        principalTable: "empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "persona_juridica",
                schema: "sujeto",
                columns: table => new
                {
                    id_persona = table.Column<Guid>(type: "uuid", nullable: false),
                    razon_social = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    fecha_creacion = table.Column<DateOnly>(type: "date", nullable: false),
                    es_grupo = table.Column<bool>(type: "boolean", nullable: false),
                    es_institucion_bancaria = table.Column<bool>(type: "boolean", nullable: false),
                    es_publica = table.Column<bool>(type: "boolean", nullable: false),
                    pais_constitucion = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_persona_juridica", x => x.id_persona);
                    table.ForeignKey(
                        name: "fk_persona_juridica_persona_id_persona",
                        column: x => x.id_persona,
                        principalSchema: "sujeto",
                        principalTable: "persona",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "persona_natural",
                schema: "sujeto",
                columns: table => new
                {
                    id_persona = table.Column<Guid>(type: "uuid", nullable: false),
                    primer_nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    segundo_nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    apellido_paterno = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    apellido_materno = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    fecha_nacimiento = table.Column<DateOnly>(type: "date", nullable: false),
                    es_masculino = table.Column<bool>(type: "boolean", nullable: false),
                    es_pep = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_persona_natural", x => x.id_persona);
                    table.ForeignKey(
                        name: "fk_persona_natural_persona_id_persona",
                        column: x => x.id_persona,
                        principalSchema: "sujeto",
                        principalTable: "persona",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "cliente",
                schema: "clientes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    id_persona = table.Column<Guid>(type: "uuid", nullable: false),
                    id_agencia = table.Column<int>(type: "integer", nullable: false),
                    id_usuario_oficial = table.Column<Guid>(type: "uuid", nullable: true),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    modificado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    modificado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cliente", x => x.id);
                    table.ForeignKey(
                        name: "fk_cliente_agencia_id_agencia",
                        column: x => x.id_agencia,
                        principalSchema: "general",
                        principalTable: "agencia",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_cliente_personas_id_persona",
                        column: x => x.id_persona,
                        principalSchema: "sujeto",
                        principalTable: "persona",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "usuario",
                schema: "seguridad",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre_usuario = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    hash_contrasena = table.Column<string>(type: "text", nullable: false),
                    id_persona = table.Column<Guid>(type: "uuid", nullable: true),
                    id_agencia = table.Column<int>(type: "integer", nullable: false),
                    puede_ingresar_sistema = table.Column<bool>(type: "boolean", nullable: false),
                    tiene_bloqueo = table.Column<bool>(type: "boolean", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    modificado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    modificado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_usuario", x => x.id);
                    table.ForeignKey(
                        name: "fk_usuario_agencia_id_agencia",
                        column: x => x.id_agencia,
                        principalSchema: "general",
                        principalTable: "agencia",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_usuario_persona_id_persona",
                        column: x => x.id_persona,
                        principalSchema: "sujeto",
                        principalTable: "persona",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "accion_ingreso_usuario",
                schema: "seguridad",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    id_usuario = table.Column<Guid>(type: "uuid", nullable: false),
                    fecha_hora = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    exitoso = table.Column<bool>(type: "boolean", nullable: false),
                    direccion_ip = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    detalle = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_accion_ingreso_usuario", x => x.id);
                    table.ForeignKey(
                        name: "fk_accion_ingreso_usuario_usuarios_id_usuario",
                        column: x => x.id_usuario,
                        principalSchema: "seguridad",
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "usuario_rol",
                schema: "seguridad",
                columns: table => new
                {
                    id_usuario = table.Column<Guid>(type: "uuid", nullable: false),
                    id_rol = table.Column<int>(type: "integer", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    asignado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_usuario_rol", x => new { x.id_usuario, x.id_rol });
                    table.ForeignKey(
                        name: "fk_usuario_rol_rol_id_rol",
                        column: x => x.id_rol,
                        principalSchema: "seguridad",
                        principalTable: "rol",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_usuario_rol_usuario_id_usuario",
                        column: x => x.id_usuario,
                        principalSchema: "seguridad",
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_accion_ingreso_usuario_fecha_hora",
                schema: "seguridad",
                table: "accion_ingreso_usuario",
                column: "fecha_hora");

            migrationBuilder.CreateIndex(
                name: "ix_accion_ingreso_usuario_id_usuario",
                schema: "seguridad",
                table: "accion_ingreso_usuario",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "ix_agencia_id_empresa_codigo",
                schema: "general",
                table: "agencia",
                columns: new[] { "id_empresa", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_cliente_id_agencia",
                schema: "clientes",
                table: "cliente",
                column: "id_agencia");

            migrationBuilder.CreateIndex(
                name: "ix_cliente_numero",
                schema: "clientes",
                table: "cliente",
                column: "numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_cliente_persona_unico_si_activo",
                schema: "clientes",
                table: "cliente",
                column: "id_persona",
                unique: true,
                filter: "estado = 'Activo'");

            migrationBuilder.CreateIndex(
                name: "ix_empresa_id_moneda",
                schema: "general",
                table: "empresa",
                column: "id_moneda");

            migrationBuilder.CreateIndex(
                name: "ix_empresa_id_pais",
                schema: "general",
                table: "empresa",
                column: "id_pais");

            migrationBuilder.CreateIndex(
                name: "ix_empresa_ruc",
                schema: "general",
                table: "empresa",
                column: "ruc",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_moneda_codigo",
                schema: "general",
                table: "moneda",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_pais_codigo",
                schema: "general",
                table: "pais",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_persona_id_pais",
                schema: "sujeto",
                table: "persona",
                column: "id_pais");

            migrationBuilder.CreateIndex(
                name: "ix_persona_id_tipo_identificacion_identificacion",
                schema: "sujeto",
                table: "persona",
                columns: new[] { "id_tipo_identificacion", "identificacion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_rol_nombre",
                schema: "seguridad",
                table: "rol",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tipo_identificacion_codigo",
                schema: "general",
                table: "tipo_identificacion",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_usuario_id_agencia",
                schema: "seguridad",
                table: "usuario",
                column: "id_agencia");

            migrationBuilder.CreateIndex(
                name: "ix_usuario_id_persona",
                schema: "seguridad",
                table: "usuario",
                column: "id_persona");

            migrationBuilder.CreateIndex(
                name: "ix_usuario_nombre_usuario",
                schema: "seguridad",
                table: "usuario",
                column: "nombre_usuario",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_usuario_rol_id_rol",
                schema: "seguridad",
                table: "usuario_rol",
                column: "id_rol");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "accion_ingreso_usuario",
                schema: "seguridad");

            migrationBuilder.DropTable(
                name: "cliente",
                schema: "clientes");

            migrationBuilder.DropTable(
                name: "persona_juridica",
                schema: "sujeto");

            migrationBuilder.DropTable(
                name: "persona_natural",
                schema: "sujeto");

            migrationBuilder.DropTable(
                name: "usuario_rol",
                schema: "seguridad");

            migrationBuilder.DropTable(
                name: "rol",
                schema: "seguridad");

            migrationBuilder.DropTable(
                name: "usuario",
                schema: "seguridad");

            migrationBuilder.DropTable(
                name: "agencia",
                schema: "general");

            migrationBuilder.DropTable(
                name: "persona",
                schema: "sujeto");

            migrationBuilder.DropTable(
                name: "empresa",
                schema: "general");

            migrationBuilder.DropTable(
                name: "tipo_identificacion",
                schema: "general");

            migrationBuilder.DropTable(
                name: "moneda",
                schema: "general");

            migrationBuilder.DropTable(
                name: "pais",
                schema: "general");
        }
    }
}

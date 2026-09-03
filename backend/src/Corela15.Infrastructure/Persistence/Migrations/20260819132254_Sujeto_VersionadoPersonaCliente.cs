using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Sujeto_VersionadoPersonaCliente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Versionado real (historico + trigger) sobre Persona/Cliente —
            // pendiente documentado desde Nivel 0 desde el diseño original,
            // mismo patrón exacto ya construido para contabilidad.cuenta_contable
            // (ver Nivel1_MotorContable.cs): nunca una copia manual, el
            // historial se llena solo en cada INSERT/UPDATE/DELETE vía trigger.
            migrationBuilder.Sql(
                """
                CREATE TABLE sujeto.persona_historico (
                    historico_id                bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                    operacion                   varchar(10) NOT NULL,
                    modificado_en               timestamptz NOT NULL DEFAULT now(),
                    id                          uuid NOT NULL,
                    identificacion              varchar(20) NOT NULL,
                    id_tipo_identificacion      integer NOT NULL,
                    nombre                      varchar(300) NOT NULL,
                    email                       varchar(200),
                    id_pais                     integer,
                    id_actividad_economica      integer,
                    activos                     numeric(18,2),
                    pasivos                     numeric(18,2),
                    ingresos                    numeric(18,2),
                    egresos                     numeric(18,2),
                    numero_casa                 text,
                    barrio                      text,
                    calle_principal             text,
                    codigo_provincia_domicilio  varchar(2),
                    creado_en                   timestamptz NOT NULL,
                    creado_por                  varchar(100) NOT NULL,
                    modificado_por              varchar(100)
                );

                CREATE INDEX ix_persona_historico_id ON sujeto.persona_historico (id);

                CREATE FUNCTION sujeto.fn_versionar_persona()
                RETURNS trigger AS $$
                BEGIN
                    IF (TG_OP = 'DELETE') THEN
                        INSERT INTO sujeto.persona_historico
                            (operacion, id, identificacion, id_tipo_identificacion, nombre, email,
                             id_pais, id_actividad_economica, activos, pasivos, ingresos, egresos,
                             numero_casa, barrio, calle_principal, codigo_provincia_domicilio,
                             creado_en, creado_por, modificado_por)
                        VALUES ('DELETE', OLD.id, OLD.identificacion, OLD.id_tipo_identificacion, OLD.nombre, OLD.email,
                                OLD.id_pais, OLD.id_actividad_economica, OLD.activos, OLD.pasivos, OLD.ingresos, OLD.egresos,
                                OLD.numero_casa, OLD.barrio, OLD.calle_principal, OLD.codigo_provincia_domicilio,
                                OLD.creado_en, OLD.creado_por, OLD.modificado_por);
                        RETURN OLD;
                    ELSE
                        INSERT INTO sujeto.persona_historico
                            (operacion, id, identificacion, id_tipo_identificacion, nombre, email,
                             id_pais, id_actividad_economica, activos, pasivos, ingresos, egresos,
                             numero_casa, barrio, calle_principal, codigo_provincia_domicilio,
                             creado_en, creado_por, modificado_por)
                        VALUES (TG_OP, NEW.id, NEW.identificacion, NEW.id_tipo_identificacion, NEW.nombre, NEW.email,
                                NEW.id_pais, NEW.id_actividad_economica, NEW.activos, NEW.pasivos, NEW.ingresos, NEW.egresos,
                                NEW.numero_casa, NEW.barrio, NEW.calle_principal, NEW.codigo_provincia_domicilio,
                                NEW.creado_en, NEW.creado_por, NEW.modificado_por);
                        RETURN NEW;
                    END IF;
                END;
                $$ LANGUAGE plpgsql;

                CREATE TRIGGER trg_versionar_persona
                    AFTER INSERT OR UPDATE OR DELETE ON sujeto.persona
                    FOR EACH ROW EXECUTE FUNCTION sujeto.fn_versionar_persona();
                """);

            migrationBuilder.Sql(
                """
                CREATE TABLE clientes.cliente_historico (
                    historico_id        bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                    operacion           varchar(10) NOT NULL,
                    modificado_en       timestamptz NOT NULL DEFAULT now(),
                    id                  uuid NOT NULL,
                    numero              varchar(20) NOT NULL,
                    id_persona          uuid NOT NULL,
                    id_agencia          integer NOT NULL,
                    id_usuario_oficial  uuid,
                    estado              varchar(20) NOT NULL,
                    creado_en           timestamptz NOT NULL,
                    creado_por          varchar(100) NOT NULL,
                    modificado_por      varchar(100)
                );

                CREATE INDEX ix_cliente_historico_id ON clientes.cliente_historico (id);

                CREATE FUNCTION clientes.fn_versionar_cliente()
                RETURNS trigger AS $$
                BEGIN
                    IF (TG_OP = 'DELETE') THEN
                        INSERT INTO clientes.cliente_historico
                            (operacion, id, numero, id_persona, id_agencia, id_usuario_oficial, estado,
                             creado_en, creado_por, modificado_por)
                        VALUES ('DELETE', OLD.id, OLD.numero, OLD.id_persona, OLD.id_agencia, OLD.id_usuario_oficial, OLD.estado,
                                OLD.creado_en, OLD.creado_por, OLD.modificado_por);
                        RETURN OLD;
                    ELSE
                        INSERT INTO clientes.cliente_historico
                            (operacion, id, numero, id_persona, id_agencia, id_usuario_oficial, estado,
                             creado_en, creado_por, modificado_por)
                        VALUES (TG_OP, NEW.id, NEW.numero, NEW.id_persona, NEW.id_agencia, NEW.id_usuario_oficial, NEW.estado,
                                NEW.creado_en, NEW.creado_por, NEW.modificado_por);
                        RETURN NEW;
                    END IF;
                END;
                $$ LANGUAGE plpgsql;

                CREATE TRIGGER trg_versionar_cliente
                    AFTER INSERT OR UPDATE OR DELETE ON clientes.cliente
                    FOR EACH ROW EXECUTE FUNCTION clientes.fn_versionar_cliente();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS trg_versionar_cliente ON clientes.cliente;");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS clientes.fn_versionar_cliente();");
            migrationBuilder.Sql("DROP TABLE IF EXISTS clientes.cliente_historico;");

            migrationBuilder.Sql("DROP TRIGGER IF EXISTS trg_versionar_persona ON sujeto.persona;");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS sujeto.fn_versionar_persona();");
            migrationBuilder.Sql("DROP TABLE IF EXISTS sujeto.persona_historico;");
        }
    }
}

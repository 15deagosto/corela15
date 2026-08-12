using Corela15.Domain.Common;

namespace Corela15.Domain.Contabilidad;

public enum NaturalezaCuenta
{
    Deudora = 1,
    Acreedora = 2
}

/// <summary>
/// Grupo de primer nivel del Catálogo Único de Cuentas (CUC) de la SEPS —
/// dígito 1 del código. Ver 02-arquitectura-datos-40-modulos.md, Nivel 1.1.
/// </summary>
public enum GrupoCuc
{
    Activo = 1,
    Pasivo = 2,
    Patrimonio = 3,
    Gastos = 4,
    Ingresos = 5,
    CuentasContingentes = 6,
    CuentasDeOrden = 7
}

/// <summary>
/// Plan de cuentas de la cooperativa — instancia del CUC con subcuentas de
/// detalle propias. Jerárquica vía IdCuentaPadre (auto-referencia), igual
/// que CONTABILIDAD.CUENTACONTABLE_PADRE en Softbank.
///
/// A diferencia de Softbank (que versiona esta tabla a mano copiando
/// CUENTACONTABLE_2022A), acá el historial real vive en
/// contabilidad.cuenta_contable_historico, poblado por trigger — ver
/// migración Nivel1_MotorContable y 02-arquitectura-datos-40-modulos.md 1.3.
/// </summary>
public class CuentaContable : AuditableEntity
{
    public Guid Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public GrupoCuc Grupo { get; set; }
    public NaturalezaCuenta Naturaleza { get; set; }

    public Guid? IdCuentaPadre { get; set; }
    public CuentaContable? CuentaPadre { get; set; }

    /// <summary>Cuenta de detalle (recibe movimientos) vs. cuenta de agrupación (solo suma).</summary>
    public bool EsMayor { get; set; }
    public bool Activa { get; set; } = true;
}

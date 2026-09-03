namespace Corela15.Application.Cajas;

public record AbrirVentanillaRequest(Guid IdUsuario, int IdAgencia);

public record VentanillaAbiertaResult(Guid IdVentanilla, DateOnly Fecha);

public record CerrarVentanillaRequest(Guid IdVentanilla, decimal TotalEfectivoContado, decimal TotalCheque, string RegistradoPor);

public record VentanillaCerradaResult(Guid IdCuadre, bool EstaCuadrado, decimal SaldoEsperado, decimal DiferenciaEfectivo);

using Corela15.Application.Mensajeria;
using Corela15.Domain.Mensajeria;
using Corela15.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Infrastructure.Services;

public class WhatsAppService(Corela15DbContext db, IWhatsAppCloudApiClient client) : IWhatsAppService
{
    public async Task<MensajeWhatsappDto> EnviarAsync(
        EnviarMensajeWhatsappRequest request, CancellationToken cancellationToken = default)
    {
        var numero = NormalizarNumero(request.NumeroDestino);

        var texto = request.Texto.Trim();
        if (texto.Length == 0 || texto.Length > 1000)
        {
            throw new TextoMensajeWhatsappInvalidoException();
        }

        string? nombrePersona = null;
        if (request.IdPersonaDestino is Guid idPersona)
        {
            nombrePersona = await db.Personas
                .Where(p => p.Id == idPersona)
                .Select(p => p.Nombre)
                .FirstOrDefaultAsync(cancellationToken);
            if (nombrePersona is null)
            {
                throw new PersonaDestinoWhatsappInvalidaException(idPersona);
            }
        }

        var nombreAgencia = await db.Agencias
            .Where(a => a.Id == request.IdAgencia && a.Activa)
            .Select(a => a.Nombre)
            .FirstOrDefaultAsync(cancellationToken);
        if (nombreAgencia is null)
        {
            throw new AgenciaInvalidaParaWhatsappException(request.IdAgencia);
        }

        // Llamada real a Meta ANTES de guardar — el resultado (éxito o
        // fallo) es lo que se audita, nunca se guarda "pendiente" (ver
        // MensajeWhatsapp: la llamada es síncrona, no hay estado en cola).
        var resultado = await client.EnviarPlantillaAsync(numero, texto, cancellationToken);

        var mensaje = new MensajeWhatsapp
        {
            Id = Guid.NewGuid(),
            NumeroDestino = numero,
            IdPersonaDestino = request.IdPersonaDestino,
            Texto = texto,
            Estado = resultado.Exitoso ? EstadoEnvioWhatsapp.Enviado : EstadoEnvioWhatsapp.Fallido,
            IdMensajeExterno = resultado.IdMensajeExterno,
            DetalleError = resultado.DetalleError,
            IdAgencia = request.IdAgencia,
            EnviadoPor = request.EnviadoPor,
            CreadoEn = DateTimeOffset.UtcNow,
        };
        db.MensajesWhatsapp.Add(mensaje);

        // Se guarda SIEMPRE, éxito o fallo — mismo criterio que
        // accion_ingreso_usuario (AuthService.LoginAsync): el intento
        // queda auditado antes de decidir si la operación lanza excepción.
        await db.SaveChangesAsync(cancellationToken);

        if (!resultado.Exitoso)
        {
            throw new WhatsAppEnvioFallidoException(resultado.DetalleError ?? "error desconocido");
        }

        return new MensajeWhatsappDto(
            mensaje.Id, mensaje.NumeroDestino, mensaje.IdPersonaDestino, nombrePersona, mensaje.Texto,
            mensaje.Estado.ToString(), mensaje.IdMensajeExterno, mensaje.DetalleError,
            nombreAgencia, mensaje.EnviadoPor, mensaje.CreadoEn);
    }

    public async Task<IReadOnlyList<MensajeWhatsappDto>> ListarAsync(
        ListarMensajesWhatsappFiltro filtro, CancellationToken cancellationToken = default)
    {
        var query = db.MensajesWhatsapp.Include(m => m.PersonaDestino).Include(m => m.Agencia).AsQueryable();

        if (filtro.Dias is int dias)
        {
            var desde = DateTimeOffset.UtcNow.AddDays(-dias);
            query = query.Where(m => m.CreadoEn >= desde);
        }

        return await query
            .OrderByDescending(m => m.CreadoEn)
            .Select(m => new MensajeWhatsappDto(
                m.Id, m.NumeroDestino, m.IdPersonaDestino,
                m.PersonaDestino != null ? m.PersonaDestino.Nombre : null,
                m.Texto, m.Estado.ToString(), m.IdMensajeExterno, m.DetalleError,
                m.Agencia.Nombre, m.EnviadoPor, m.CreadoEn))
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Ecuador por defecto: "0987654321" (10 dígitos, 0 inicial) o
    /// "987654321" (9 dígitos, sin 0) se convierten a E.164 real
    /// (593987654321). Un número que ya trae código de país (ej. desde
    /// otro país) se deja tal cual, solo se limpian espacios/guiones/+.
    /// </summary>
    private static string NormalizarNumero(string numero)
    {
        var digitos = new string((numero ?? string.Empty).Where(char.IsDigit).ToArray());

        if (digitos.Length == 10 && digitos.StartsWith('0'))
        {
            digitos = "593" + digitos[1..];
        }
        else if (digitos.Length == 9)
        {
            digitos = "593" + digitos;
        }

        if (digitos.Length is < 9 or > 15)
        {
            throw new NumeroWhatsappInvalidoException(numero ?? string.Empty);
        }

        return digitos;
    }
}

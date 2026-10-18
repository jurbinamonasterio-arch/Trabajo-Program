namespace Sigbim.BLL;

/// <summary>
/// Comportamiento común de los servicios de negocio: registra en la bitácora
/// cada operación exitosa o fallida y relanza la excepción controlada a la UI.
/// </summary>
public abstract class ServicioBase
{
    protected static T Ejecutar<T>(string operacion, Func<T> accion, Func<T, string>? detalleExito = null)
    {
        try
        {
            var resultado = accion();
            if (detalleExito is not null)
                RegistroEventos.Info(operacion, detalleExito(resultado));
            return resultado;
        }
        catch (Exception ex)
        {
            RegistroEventos.Error(operacion, ex);
            throw;
        }
    }
}

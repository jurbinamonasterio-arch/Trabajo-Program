using Sigbim.DAL;

namespace Sigbim.BLL;

/// <summary>Operaciones generales del sistema expuestas a la capa de presentación.</summary>
public static class SistemaBLL
{
    /// <summary>
    /// Verifica que la base de datos esté disponible. La UI la usa al iniciar,
    /// sin conocer la capa de acceso a datos.
    /// </summary>
    public static void ProbarConexion()
    {
        try
        {
            Conexion.Probar();
            RegistroEventos.Info("Inicio de sesión", "Conexión a base de datos verificada");
        }
        catch (Exception ex)
        {
            RegistroEventos.Error("Inicio de sesión", ex);
            throw;
        }
    }
}

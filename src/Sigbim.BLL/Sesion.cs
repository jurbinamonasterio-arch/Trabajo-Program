namespace Sigbim.BLL;

/// <summary>
/// Identifica al usuario que opera la aplicación. Se registra en cada operación
/// (UsuarioCreacion / UsuarioModificacion / UsuarioResponsable) para la trazabilidad
/// (ISO 27001 A.8.15). Por defecto toma el usuario de la sesión del sistema operativo.
/// </summary>
public static class Sesion
{
    private static string _usuario = Recortar(Environment.UserName);

    public static string UsuarioActual
    {
        get => _usuario;
        set => _usuario = string.IsNullOrWhiteSpace(value) ? _usuario : Recortar(value.Trim());
    }

    private static string Recortar(string valor) => valor.Length <= 50 ? valor : valor[..50];
}

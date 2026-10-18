using System.Data.Common;
using MySqlConnector;
using Sigbim.Entidades;

namespace Sigbim.DAL;

/// <summary>
/// Convierte los errores técnicos de MySQL en mensajes claros para el usuario,
/// sin exponer nombres de tablas, consultas ni detalles del servidor.
/// </summary>
internal static class TraductorErrores
{
    // Códigos de error de MySQL / MariaDB
    private const int ClaveDuplicada = 1062;
    private const int FkFilaReferenciada = 1451;
    private const int FkPadreNoExiste = 1452;
    private const int ErrorSignal = 1644;          // SIGNAL SQLSTATE '45000' desde los SPs
    private const int AccesoDenegado = 1045;
    private const int SinPermisoComando = 1142;
    private const int SinPermisoRutina = 1370;
    private const int DatoMuyLargo = 1406;
    private const int CheckViolado = 3819;         // MySQL 8
    private const int CheckVioladoMaria = 4025;    // MariaDB
    private const int NoConectaHost = 1042;
    private const int NoConectaCliente = 2002;
    private const int NoConectaServidor = 2003;

    public static DatosException Traducir(DbException ex)
    {
        var codigo = ex is MySqlException mysql ? mysql.Number : 0;

        var mensaje = codigo switch
        {
            ErrorSignal => ex.Message,  // Mensaje de negocio escrito en el SP
            ClaveDuplicada => "Ya existe un registro con ese valor (SKU, RUT o nombre duplicado).",
            FkFilaReferenciada => "No se puede eliminar: el registro está siendo usado por otros datos.",
            FkPadreNoExiste => "La categoría o el proveedor seleccionado no existe.",
            CheckViolado or CheckVioladoMaria => "Los datos no cumplen las reglas de la base de datos (valores negativos o tipo inválido).",
            DatoMuyLargo => "Uno de los textos supera el largo máximo permitido.",
            AccesoDenegado => "Usuario o contraseña de base de datos incorrectos.",
            SinPermisoComando or SinPermisoRutina => "El usuario de la aplicación no tiene permisos para esta operación.",
            NoConectaHost or NoConectaCliente or NoConectaServidor =>
                "No se pudo conectar con el servidor MySQL. Verifica que esté encendido.",
            _ => "Ocurrió un error inesperado en la base de datos."
        };

        return new DatosException(mensaje, codigo, ex);
    }
}

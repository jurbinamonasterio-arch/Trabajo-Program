using System.Data.Common;
using System.Text.Json;
using MySqlConnector;
using Sigbim.Entidades;

namespace Sigbim.DAL;

/// <summary>
/// Entrega conexiones a la base de datos. La cadena de conexión NO está escrita
/// en el código (ISO 27001 A.5.17 / A.8.4): se lee desde la variable de entorno
/// SIGBIM_CONEXION o desde el archivo appsettings.json, que está excluido de Git.
/// </summary>
public static class Conexion
{
    private const string VariableEntorno = "SIGBIM_CONEXION";
    private const string ArchivoConfig = "appsettings.json";
    private static string? _cadena;

    internal static DbProviderFactory Fabrica => MySqlConnectorFactory.Instance;

    public static string CadenaConexion => _cadena ??= LeerCadena();

    internal static DbConnection Crear()
    {
        var conexion = Fabrica.CreateConnection()
                       ?? throw new DatosException("No se pudo crear la conexión a la base de datos.");
        conexion.ConnectionString = CadenaConexion;
        return conexion;
    }

    /// <summary>Permite verificar la conexión al iniciar la aplicación.</summary>
    public static void Probar()
    {
        try
        {
            using var conexion = Crear();
            conexion.Open();
        }
        catch (DbException ex)
        {
            throw TraductorErrores.Traducir(ex);
        }
    }

    private static string LeerCadena()
    {
        var desdeEntorno = Environment.GetEnvironmentVariable(VariableEntorno);
        if (!string.IsNullOrWhiteSpace(desdeEntorno))
            return desdeEntorno;

        var ruta = Path.Combine(AppContext.BaseDirectory, ArchivoConfig);
        if (!File.Exists(ruta))
            throw new DatosException(
                "No se encontró el archivo appsettings.json. Copia appsettings.example.json " +
                "como appsettings.json y completa los datos de conexión.");

        using var json = JsonDocument.Parse(File.ReadAllText(ruta));
        if (json.RootElement.TryGetProperty("ConnectionStrings", out var cadenas) &&
            cadenas.TryGetProperty("Sigbim", out var cadena) &&
            !string.IsNullOrWhiteSpace(cadena.GetString()))
        {
            return cadena.GetString()!;
        }

        throw new DatosException("El archivo appsettings.json no contiene ConnectionStrings:Sigbim.");
    }
}

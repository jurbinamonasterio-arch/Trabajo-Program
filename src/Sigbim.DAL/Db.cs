using System.Data;
using System.Data.Common;

namespace Sigbim.DAL;

/// <summary>
/// Utilidades comunes de acceso a datos. Todas las operaciones se ejecutan como
/// procedimientos almacenados (CALL) con parámetros, nunca concatenando datos
/// en el SQL (prevención de inyección SQL, ISO 27001 A.8.28).
/// </summary>
internal static class Db
{
    public static List<T> Lista<T>(string procedimiento, Func<DbDataReader, T> mapear,
                                   params (string Nombre, object? Valor)[] parametros)
    {
        return Ejecutar(procedimiento, parametros, cmd =>
        {
            var resultado = new List<T>();
            using var lector = cmd.ExecuteReader();
            while (lector.Read())
                resultado.Add(mapear(lector));
            return resultado;
        });
    }

    public static T? Primero<T>(string procedimiento, Func<DbDataReader, T> mapear,
                                params (string Nombre, object? Valor)[] parametros) where T : class
    {
        return Lista(procedimiento, mapear, parametros).FirstOrDefault();
    }

    public static int EscalarEntero(string procedimiento, params (string Nombre, object? Valor)[] parametros)
    {
        return Ejecutar(procedimiento, parametros, cmd => Convert.ToInt32(cmd.ExecuteScalar()));
    }

    private static T Ejecutar<T>(string procedimiento, (string Nombre, object? Valor)[] parametros,
                                 Func<DbCommand, T> accion)
    {
        try
        {
            using var conexion = Conexion.Crear();
            conexion.Open();
            using var cmd = conexion.CreateCommand();

            // CALL sp_nombre(@p_A, @p_B, ...): solo se invocan procedimientos almacenados.
            // El nombre del SP es una constante del código (nunca un dato del usuario) y
            // todos los valores viajan como parámetros, por lo que no hay inyección SQL.
            // Los parámetros se pasan en el mismo orden en que están declarados en el SP.
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = $"CALL {procedimiento}({string.Join(", ", parametros.Select(p => "@" + p.Nombre))})";

            foreach (var (nombre, valor) in parametros)
            {
                var p = cmd.CreateParameter();
                p.ParameterName = "@" + nombre;
                p.Value = valor ?? DBNull.Value;
                cmd.Parameters.Add(p);
            }

            return accion(cmd);
        }
        catch (DbException ex)
        {
            throw TraductorErrores.Traducir(ex);
        }
    }

    // ---------- Lectura segura de columnas ----------

    public static string Texto(this DbDataReader r, string col) =>
        r[col] is DBNull ? string.Empty : Convert.ToString(r[col])!;

    public static string? TextoNulo(this DbDataReader r, string col) =>
        r[col] is DBNull ? null : Convert.ToString(r[col]);

    public static int Entero(this DbDataReader r, string col) =>
        r[col] is DBNull ? 0 : Convert.ToInt32(r[col]);

    public static decimal Decimal(this DbDataReader r, string col) =>
        r[col] is DBNull ? 0m : Convert.ToDecimal(r[col]);

    public static bool Booleano(this DbDataReader r, string col) =>
        r[col] is not DBNull && Convert.ToBoolean(r[col]);

    public static DateTime Fecha(this DbDataReader r, string col) =>
        r[col] is DBNull ? DateTime.MinValue : Convert.ToDateTime(r[col]);

    public static DateTime? FechaNula(this DbDataReader r, string col) =>
        r[col] is DBNull ? null : Convert.ToDateTime(r[col]);
}

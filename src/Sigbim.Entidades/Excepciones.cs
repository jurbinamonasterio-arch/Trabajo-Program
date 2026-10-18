namespace Sigbim.Entidades;

/// <summary>Base de las excepciones controladas del sistema.</summary>
public abstract class SigbimException : Exception
{
    protected SigbimException(string mensaje, Exception? interna = null) : base(mensaje, interna) { }
}

/// <summary>
/// Errores de reglas de negocio o de datos ingresados por el usuario (lanzada por la BLL).
/// Contiene la lista completa de problemas para mostrarlos todos de una vez.
/// </summary>
public class ValidacionException : SigbimException
{
    public IReadOnlyList<string> Errores { get; }

    public ValidacionException(IEnumerable<string> errores)
        : base("Los datos ingresados no son válidos.")
    {
        Errores = errores.ToList();
    }

    public ValidacionException(string error) : this(new[] { error }) { }
}

/// <summary>
/// Error proveniente de la base de datos (lanzada por la DAL), con un mensaje
/// comprensible para el usuario. El detalle técnico queda en InnerException
/// y solo se escribe en el log (ISO 27001 A.8.12: no exponer información interna).
/// </summary>
public class DatosException : SigbimException
{
    public int CodigoError { get; }

    public DatosException(string mensaje, int codigoError = 0, Exception? interna = null)
        : base(mensaje, interna)
    {
        CodigoError = codigoError;
    }
}

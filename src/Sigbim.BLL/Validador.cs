using System.Text.RegularExpressions;
using Sigbim.Entidades;

namespace Sigbim.BLL;

/// <summary>
/// Acumula los errores de validación de una entidad para informarlos todos juntos.
/// Implementa la validación de entradas exigida por ISO 27001 A.8.28 (codificación segura).
/// </summary>
internal class Validador
{
    private readonly List<string> _errores = new();

    private static readonly Regex RegexEmail =
        new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
    private static readonly Regex RegexSku =
        new(@"^[A-Z0-9\-]{3,20}$", RegexOptions.Compiled);
    private static readonly Regex RegexTelefono =
        new(@"^\+?[0-9 ]{8,20}$", RegexOptions.Compiled);

    public Validador Requerido(string? valor, string campo)
    {
        if (string.IsNullOrWhiteSpace(valor))
            _errores.Add($"{campo} es obligatorio.");
        return this;
    }

    public Validador LargoMaximo(string? valor, int maximo, string campo)
    {
        if (valor is not null && valor.Trim().Length > maximo)
            _errores.Add($"{campo} no puede superar {maximo} caracteres.");
        return this;
    }

    public Validador Email(string? valor, string campo)
    {
        if (!string.IsNullOrWhiteSpace(valor) && !RegexEmail.IsMatch(valor.Trim()))
            _errores.Add($"{campo} no tiene un formato válido.");
        return this;
    }

    public Validador Telefono(string? valor, string campo)
    {
        if (!string.IsNullOrWhiteSpace(valor) && !RegexTelefono.IsMatch(valor.Trim()))
            _errores.Add($"{campo} solo admite números, espacios y '+' (8 a 20 caracteres).");
        return this;
    }

    public Validador Sku(string? valor)
    {
        if (!string.IsNullOrWhiteSpace(valor) && !RegexSku.IsMatch(valor.Trim().ToUpperInvariant()))
            _errores.Add("SKU debe tener entre 3 y 20 caracteres: letras, números o guion.");
        return this;
    }

    public Validador Rut(string? valor)
    {
        if (!string.IsNullOrWhiteSpace(valor) && !RutValido(valor))
            _errores.Add("RUT no es válido (formato 12345678-9, dígito verificador incorrecto).");
        return this;
    }

    public Validador Condicion(bool cumple, string mensaje)
    {
        if (!cumple)
            _errores.Add(mensaje);
        return this;
    }

    public void LanzarSiHayErrores()
    {
        if (_errores.Count > 0)
            throw new ValidacionException(_errores);
    }

    /// <summary>Normaliza un RUT a formato 12345678-9 (sin puntos, DV en mayúscula).</summary>
    public static string NormalizarRut(string rut)
    {
        var limpio = rut.Replace(".", "").Replace(" ", "").ToUpperInvariant();
        if (!limpio.Contains('-') && limpio.Length > 1)
            limpio = limpio[..^1] + "-" + limpio[^1];
        return limpio;
    }

    /// <summary>Valida el dígito verificador del RUT chileno (algoritmo módulo 11).</summary>
    public static bool RutValido(string rut)
    {
        var normalizado = NormalizarRut(rut);
        var partes = normalizado.Split('-');
        if (partes.Length != 2 || partes[1].Length != 1 || !int.TryParse(partes[0], out var cuerpo) || cuerpo <= 0)
            return false;

        int suma = 0, multiplicador = 2;
        for (var n = cuerpo; n > 0; n /= 10)
        {
            suma += (n % 10) * multiplicador;
            multiplicador = multiplicador == 7 ? 2 : multiplicador + 1;
        }

        var resto = 11 - (suma % 11);
        var dvEsperado = resto switch { 11 => "0", 10 => "K", _ => resto.ToString() };
        return partes[1] == dvEsperado;
    }
}

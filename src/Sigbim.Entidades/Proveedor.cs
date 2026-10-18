namespace Sigbim.Entidades;

/// <summary>Empresa que abastece productos (tabla Proveedor).</summary>
public class Proveedor : EntidadAuditable
{
    public int IdProveedor { get; set; }
    public string Rut { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string? NombreContacto { get; set; }
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public string? Ciudad { get; set; }

    public override string ToString() => $"{RazonSocial} ({Rut})";
}

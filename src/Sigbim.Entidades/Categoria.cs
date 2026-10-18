namespace Sigbim.Entidades;

/// <summary>Clasificación de productos (tabla Categoria).</summary>
public class Categoria : EntidadAuditable
{
    public int IdCategoria { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }

    public override string ToString() => Nombre;
}

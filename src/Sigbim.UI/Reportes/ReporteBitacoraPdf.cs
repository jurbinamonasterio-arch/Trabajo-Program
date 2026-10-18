using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Sigbim.BLL;
using Sigbim.Entidades;

namespace Sigbim.UI.Reportes;

/// <summary>
/// Genera el reporte PDF de la bitácora: resumen (por usuario y por operación)
/// y detalle cronológico de todos los eventos registrados.
/// </summary>
public static class ReporteBitacoraPdf
{
    static ReporteBitacoraPdf()
    {
        // Licencia gratuita de QuestPDF para uso académico / organizaciones pequeñas
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public static void Generar(string ruta, IReadOnlyList<EventoBitacora> eventos)
    {
        var porUsuario = eventos.GroupBy(e => e.Usuario).OrderBy(g => g.Key).ToList();
        var porOperacion = eventos.GroupBy(e => e.Operacion).OrderByDescending(g => g.Count()).ToList();
        var desde = eventos.Count > 0 ? eventos[0].Fecha : (DateTime?)null;
        var hasta = eventos.Count > 0 ? eventos[^1].Fecha : (DateTime?)null;

        Document.Create(doc => doc.Page(page =>
        {
            page.Size(PageSizes.A4.Landscape());
            page.Margin(28);
            page.DefaultTextStyle(t => t.FontSize(9));

            page.Header().Column(col =>
            {
                col.Item().Text("SIGBIM - Reporte de bitácora").FontSize(18).Bold();
                col.Item().Text("Sistema de Gestión de Bodega de Insumos Médicos · Registro de eventos (ISO/IEC 27001 A.8.15)").FontColor(Colors.Grey.Darken1);
                col.Item().PaddingTop(4).Text(
                    $"Generado: {DateTime.Now:dd-MM-yyyy HH:mm} por {Sesion.UsuarioActual}" +
                    (desde is null ? "" : $"   ·   Período: {desde:dd-MM-yyyy HH:mm} a {hasta:dd-MM-yyyy HH:mm}"));
                col.Item().PaddingTop(6).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
            });

            page.Content().PaddingTop(10).Column(col =>
            {
                col.Spacing(12);

                // ---------- Resumen ----------
                col.Item().Text("1. Resumen").FontSize(12).Bold();
                col.Item().Text(
                    $"Total de eventos: {eventos.Count}   ·   " +
                    $"Inicios de sesión: {eventos.Count(e => e.Operacion == "Inicio de sesión")}   ·   " +
                    $"Advertencias: {eventos.Count(e => e.Nivel == "WARN")}   ·   " +
                    $"Errores: {eventos.Count(e => e.Nivel == "ERROR")}");

                col.Item().Row(row =>
                {
                    row.Spacing(16);
                    row.RelativeItem().Element(c => TablaResumen(c, "Usuario", porUsuario));
                    row.RelativeItem().Element(c => TablaResumen(c, "Operación", porOperacion));
                });

                // ---------- Detalle ----------
                col.Item().Text("2. Detalle de eventos").FontSize(12).Bold();
                col.Item().Table(t =>
                {
                    t.ColumnsDefinition(c =>
                    {
                        c.ConstantColumn(95);   // Fecha
                        c.ConstantColumn(45);   // Nivel
                        c.ConstantColumn(90);   // Usuario
                        c.RelativeColumn(2);    // Operación
                        c.RelativeColumn(4);    // Detalle
                    });

                    t.Header(h =>
                    {
                        foreach (var titulo in new[] { "Fecha", "Nivel", "Usuario", "Operación", "Detalle" })
                            h.Cell().Element(Encabezado).Text(titulo).Bold();
                    });

                    foreach (var e in eventos)
                    {
                        t.Cell().Element(Celda).Text($"{e.Fecha:dd-MM-yyyy HH:mm:ss}");
                        t.Cell().Element(Celda).Text(e.Nivel).FontColor(e.Nivel switch
                        {
                            "ERROR" => Colors.Red.Darken2,
                            "WARN" => Colors.Orange.Darken2,
                            _ => Colors.Black
                        });
                        t.Cell().Element(Celda).Text(e.Usuario);
                        t.Cell().Element(Celda).Text(e.Operacion);
                        t.Cell().Element(Celda).Text(e.Detalle);
                    }
                });

                if (eventos.Count == 0)
                    col.Item().Text("No hay eventos registrados.").Italic();
            });

            page.Footer().AlignRight().Text(t =>
            {
                t.Span("Página ");
                t.CurrentPageNumber();
                t.Span(" de ");
                t.TotalPages();
            });
        })).GeneratePdf(ruta);
    }

    private static void TablaResumen(IContainer contenedor, string titulo, IEnumerable<IGrouping<string, EventoBitacora>> grupos)
    {
        contenedor.Table(t =>
        {
            t.ColumnsDefinition(c =>
            {
                c.RelativeColumn();
                c.ConstantColumn(60);
            });
            t.Header(h =>
            {
                h.Cell().Element(Encabezado).Text(titulo).Bold();
                h.Cell().Element(Encabezado).AlignRight().Text("Eventos").Bold();
            });
            foreach (var g in grupos)
            {
                t.Cell().Element(Celda).Text(g.Key);
                t.Cell().Element(Celda).AlignRight().Text(g.Count().ToString());
            }
        });
    }

    private static IContainer Encabezado(IContainer c) =>
        c.Background(Colors.Grey.Lighten3).Padding(4);

    private static IContainer Celda(IContainer c) =>
        c.BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(3).PaddingHorizontal(4);
}

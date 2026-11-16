using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TechStore.Models;
using TechStore.Services;

namespace TechStore.Controllers
{
    // Unidad 3: reportes de productos
    // - PDF con QuestPDF
    // - Excel con ClosedXML
    // Solo el Administrador y el Vendedor pueden generar reportes
    [Authorize(Roles = "Administrador,Vendedor")]
    public class ReportesController : Controller
    {
        private readonly IProductoService _productos;
        private readonly ILogger<ReportesController> _logger;

        public ReportesController(IProductoService productos, ILogger<ReportesController> logger)
        {
            _productos = productos;
            _logger = logger;
        }

        // =====================================================================
        // PDF: abrir en el navegador (sin nombre de archivo el navegador lo muestra)
        // =====================================================================
        public async Task<IActionResult> VerPdf()
        {
            try
            {
                // Los datos se leen de la base de datos con Entity Framework Core (incluye la categoría)
                var productos = await _productos.ObtenerTodos();
                _logger.LogInformation("Reporte PDF de productos generado con {Cantidad} registros", productos.Count);
                return File(GenerarReportePdf(productos), "application/pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar el reporte PDF de productos");
                throw;
            }
        }

        // =====================================================================
        // PDF: descargar (el tercer parámetro de File() es el nombre del archivo)
        // =====================================================================
        public async Task<IActionResult> DescargarPdf()
        {
            try
            {
                var productos = await _productos.ObtenerTodos();
                var nombreArchivo = $"ReporteProductos_TechStore_{DateTime.Now:yyyyMMdd}.pdf";
                _logger.LogInformation("Descargando el reporte PDF {Archivo}", nombreArchivo);
                return File(GenerarReportePdf(productos), "application/pdf", nombreArchivo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al descargar el reporte PDF de productos");
                throw;
            }
        }

        // =====================================================================
        // EXCEL: descargar el archivo .xlsx
        // =====================================================================
        public async Task<IActionResult> ExportarExcel()
        {
            try
            {
                var productos = await _productos.ObtenerTodos();
                var nombreArchivo = $"ReporteProductos_TechStore_{DateTime.Now:yyyyMMdd}.xlsx";
                _logger.LogInformation("Exportando los productos a Excel {Archivo}", nombreArchivo);

                // Tipo de contenido oficial de los archivos de Excel (.xlsx)
                return File(GenerarReporteExcel(productos),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    nombreArchivo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al exportar los productos a Excel");
                throw;
            }
        }

        // =====================================================================
        // DISEÑO DEL PDF CON QUESTPDF
        // Estructura: Document > Page > Header / Content / Footer
        // =====================================================================
        private byte[] GenerarReportePdf(IReadOnlyCollection<Producto> productos)
        {
            var activos = productos.Count(p => p.Estado);
            var unidades = productos.Sum(p => p.Stock);
            var usuario = User.Identity?.Name ?? "";

            return Document.Create(document => document.Page(page =>
            {
                // Horizontal porque son 7 columnas
                page.Size(PageSizes.Letter.Landscape());
                page.Margin(36);
                page.PageColor(Colors.White);

                // ---------------- ENCABEZADO ----------------
                page.Header().Column(column =>
                {
                    // Nombre de la tienda
                    column.Item().Text("TECHSTORE")
                        .FontSize(12)
                        .Bold()
                        .FontColor(Colors.Blue.Darken2);

                    // Título del reporte
                    column.Item().Text("Reporte de productos")
                        .FontSize(24)
                        .Bold()
                        .FontColor(Colors.Grey.Darken3);

                    column.Item().Text($"Generado el {DateTime.Now:dd/MM/yyyy HH:mm} por {usuario}")
                        .FontSize(9)
                        .FontColor(Colors.Grey.Darken1);

                    column.Item().PaddingTop(10).LineHorizontal(2).LineColor(Colors.Blue.Medium);
                });

                // ---------------- CONTENIDO ----------------
                page.Content().PaddingVertical(16).Column(column =>
                {
                    // Recuadro azul con el total de productos
                    column.Item().Background(Colors.Blue.Darken2).Padding(12).Row(row =>
                    {
                        row.RelativeItem().Text("Productos registrados")
                            .FontSize(11)
                            .Bold()
                            .FontColor(Colors.White);

                        row.AutoItem().Text(productos.Count.ToString())
                            .FontSize(14)
                            .Bold()
                            .FontColor(Colors.White);
                    });

                    // Resumen: activos, inactivos y unidades en stock
                    column.Item().PaddingTop(6).Row(row =>
                    {
                        row.RelativeItem().Text($"Activos: {activos}").FontSize(9).Bold().FontColor(Colors.Green.Darken2);
                        row.RelativeItem().AlignCenter().Text($"Inactivos: {productos.Count - activos}").FontSize(9).Bold().FontColor(Colors.Red.Darken2);
                        row.RelativeItem().AlignRight().Text($"Unidades en stock: {unidades}").FontSize(9).Bold().FontColor(Colors.Grey.Darken2);
                    });

                    if (productos.Count == 0)
                    {
                        column.Item().PaddingTop(16).Text("No hay productos registrados.").FontSize(11);
                        return;
                    }

                    // Tabla de productos
                    column.Item().PaddingTop(14).Table(table =>
                    {
                        // El ID tiene ancho fijo y las demás columnas se reparten el espacio
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(32);     // ID
                            columns.RelativeColumn(2.2f);   // Nombre
                            columns.RelativeColumn(3.2f);   // Descripción (la más ancha)
                            columns.RelativeColumn(1f);     // Precio
                            columns.RelativeColumn(1.3f);   // Categoría
                            columns.RelativeColumn(0.8f);   // Stock
                            columns.RelativeColumn(1f);     // Estado
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(EstiloEncabezado).Text("ID");
                            header.Cell().Element(EstiloEncabezado).Text("NOMBRE");
                            header.Cell().Element(EstiloEncabezado).Text("DESCRIPCIÓN");
                            header.Cell().Element(EstiloEncabezado).AlignRight().Text("PRECIO");
                            header.Cell().Element(EstiloEncabezado).Text("CATEGORÍA");
                            header.Cell().Element(EstiloEncabezado).AlignCenter().Text("STOCK");
                            header.Cell().Element(EstiloEncabezado).AlignCenter().Text("ESTADO");
                        });

                        foreach (var producto in productos)
                        {
                            table.Cell().Element(EstiloCelda).Text(producto.ID.ToString());
                            table.Cell().Element(EstiloCelda).Text(producto.Nombre);
                            table.Cell().Element(EstiloCelda).Text(producto.Descripcion);
                            table.Cell().Element(EstiloCelda).AlignRight().Text($"${producto.Precio:N2}");
                            table.Cell().Element(EstiloCelda).Text(producto.Categoria?.Nombre ?? "");
                            table.Cell().Element(EstiloCelda).AlignCenter().Text(producto.Stock.ToString());

                            // El estado cambia de color: verde activo, rojo inactivo
                            table.Cell().Element(EstiloCelda).AlignCenter()
                                .Text(producto.Estado ? "Activo" : "Inactivo")
                                .FontColor(producto.Estado ? Colors.Green.Darken2 : Colors.Red.Darken2);
                        }
                    });
                });

                // ---------------- PIE DE PÁGINA ----------------
                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("TechStore - Página ");
                    text.CurrentPageNumber();
                    text.Span(" de ");
                    text.TotalPages();
                });
            })).GeneratePdf();   // convierte el documento en byte[]
        }

        // Estilo de las celdas del encabezado de la tabla
        static IContainer EstiloEncabezado(IContainer container) => container
            .Background(Colors.Blue.Darken2)
            .PaddingVertical(7)
            .PaddingHorizontal(5)
            .DefaultTextStyle(text => text.FontSize(8).Bold().FontColor(Colors.White));

        // Estilo de las celdas con los datos
        static IContainer EstiloCelda(IContainer container) => container
            .BorderBottom(1)
            .BorderColor(Colors.Grey.Lighten2)
            .PaddingVertical(6)
            .PaddingHorizontal(5)
            .DefaultTextStyle(text => text.FontSize(8).FontColor(Colors.Grey.Darken3));

        // =====================================================================
        // DISEÑO DEL EXCEL CON CLOSEDXML
        // XLWorkbook = libro, Worksheet = hoja, Cell = celda, Range = rango
        // =====================================================================
        private static byte[] GenerarReporteExcel(IReadOnlyCollection<Producto> productos)
        {
            var azul = XLColor.FromHtml("#1565C0");

            using var libro = new XLWorkbook();
            var hoja = libro.Worksheets.Add("Productos");

            // ---------------- ENCABEZADO DEL REPORTE ----------------
            hoja.Cell(1, 1).Value = "TECHSTORE";
            hoja.Range(1, 1, 1, 7).Merge();
            hoja.Cell(1, 1).Style.Font.Bold = true;
            hoja.Cell(1, 1).Style.Font.FontColor = azul;

            hoja.Cell(2, 1).Value = "Reporte de productos";
            hoja.Range(2, 1, 2, 7).Merge();
            hoja.Cell(2, 1).Style.Font.Bold = true;
            hoja.Cell(2, 1).Style.Font.FontSize = 16;

            hoja.Cell(3, 1).Value = $"Generado el {DateTime.Now:dd/MM/yyyy HH:mm}";
            hoja.Range(3, 1, 3, 7).Merge();
            hoja.Cell(3, 1).Style.Font.FontColor = XLColor.Gray;

            // ---------------- ENCABEZADOS DE LA TABLA (fila 5) ----------------
            int filaEncabezado = 5;
            string[] columnas = { "ID", "Nombre", "Descripción", "Precio", "Categoría", "Stock", "Estado" };
            for (int i = 0; i < columnas.Length; i++)
            {
                hoja.Cell(filaEncabezado, i + 1).Value = columnas[i];
            }

            var rangoEncabezado = hoja.Range(filaEncabezado, 1, filaEncabezado, columnas.Length);
            rangoEncabezado.Style.Font.Bold = true;
            rangoEncabezado.Style.Font.FontColor = XLColor.White;
            rangoEncabezado.Style.Fill.BackgroundColor = azul;
            rangoEncabezado.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // ---------------- DATOS ----------------
            int fila = filaEncabezado + 1;
            foreach (var producto in productos)
            {
                hoja.Cell(fila, 1).Value = producto.ID;
                hoja.Cell(fila, 2).Value = producto.Nombre;
                hoja.Cell(fila, 3).Value = producto.Descripcion;
                hoja.Cell(fila, 4).Value = producto.Precio;   // número, para poder sumar en Excel
                hoja.Cell(fila, 5).Value = producto.Categoria?.Nombre ?? "";
                hoja.Cell(fila, 6).Value = producto.Stock;
                hoja.Cell(fila, 7).Value = producto.Estado ? "Activo" : "Inactivo";
                hoja.Cell(fila, 7).Style.Font.FontColor = producto.Estado ? XLColor.DarkGreen : XLColor.DarkRed;
                fila++;
            }
            int ultimaFila = Math.Max(fila - 1, filaEncabezado);

            // Formato: precio como moneda, ID/Stock/Estado centrados y bordes en toda la tabla
            if (productos.Count > 0)
            {
                hoja.Range(filaEncabezado + 1, 4, ultimaFila, 4).Style.NumberFormat.Format = "$#,##0.00";
                hoja.Range(filaEncabezado + 1, 1, ultimaFila, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                hoja.Range(filaEncabezado + 1, 6, ultimaFila, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                hoja.Range(filaEncabezado + 1, 3, ultimaFila, 3).Style.Alignment.WrapText = true;
            }

            var rangoTabla = hoja.Range(filaEncabezado, 1, ultimaFila, columnas.Length);
            rangoTabla.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            rangoTabla.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            rangoTabla.SetAutoFilter();

            // ---------------- RESUMEN ----------------
            int filaResumen = ultimaFila + 2;
            hoja.Cell(filaResumen, 1).Value = "Total de productos:";
            hoja.Cell(filaResumen, 3).Value = productos.Count;
            hoja.Cell(filaResumen + 1, 1).Value = "Unidades en stock:";
            hoja.Cell(filaResumen + 1, 3).Value = productos.Sum(p => p.Stock);
            hoja.Range(filaResumen, 1, filaResumen + 1, 3).Style.Font.Bold = true;
            hoja.Range(filaResumen, 3, filaResumen + 1, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

            // Ancho de columnas: automático + espacio para la flecha del autofiltro
            hoja.Columns(1, columnas.Length).AdjustToContents(filaEncabezado, ultimaFila);
            foreach (var columna in hoja.Columns(1, columnas.Length))
            {
                columna.Width += 3;
            }
            // La descripción puede ser larga: ancho fijo y texto en varias líneas
            hoja.Column(3).Width = 50;

            hoja.SheetView.FreezeRows(filaEncabezado);

            // Se guarda el libro en memoria y se devuelve como byte[] (igual que el PDF)
            using var memoria = new MemoryStream();
            libro.SaveAs(memoria);
            return memoria.ToArray();
        }
    }
}

namespace TechStore.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        // Unidad 3: código HTTP del error (404, 400...). Si viene vacío es una excepción no controlada
        public int? CodigoEstado { get; set; }
    }
}

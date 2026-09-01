using System.ComponentModel.DataAnnotations;

namespace POS_Entidades
{
    public class Producto
    {
        public int IdProducto { get; set; }

        // Limitamos la longitud para proteger el VARCHAR de la base de datos
        [MaxLength(50, ErrorMessage = "El código no puede exceder los 50 caracteres.")]
        public string Codigo { get; set; } = string.Empty;

        [MaxLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        public decimal PrecioCosto { get; set; }
        public decimal PrecioVenta { get; set; }

        // PROPIEDAD CALCULADA: Margen de ganancia exacto por unidad
        public decimal Ganancia => PrecioVenta - PrecioCosto;

        public decimal Stock { get; set; }
        public decimal StockMinimo { get; set; }
        public bool SeVendePorUnidad { get; set; }

        [MaxLength(50)]
        public string Marca { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Proveedor { get; set; } = string.Empty;

        public bool ControlaStock { get; set; }
        public decimal StockIdeal { get; set; }
    }
}
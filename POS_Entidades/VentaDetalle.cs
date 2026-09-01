using System;

namespace POS_Entidades
{
    public class VentaDetalle
    {
        public int IdDetalle { get; set; }
        public int IdVenta { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public decimal Cantidad { get; set; }

        // ¡Estas dos son la clave!
        public decimal PrecioCosto { get; set; }
        public decimal PrecioVenta { get; set; }

        public decimal Subtotal { get; set; }
        public bool FueDevuelto { get; set; }
        public decimal CantidadDevuelta { get; set; }

        // ==============================================================
        // NUEVAS PROPIEDADES CALCULADAS FINANCIERAS
        // ==============================================================

        /// <summary>
        /// Obtiene la ganancia neta en dinero por cada unidad de este artículo.
        /// </summary>
        public decimal GananciaNetaUnidad => PrecioVenta - PrecioCosto;

        /// <summary>
        /// Obtiene el porcentaje de margen de rentabilidad del artículo.
        /// Evita la división entre cero si el precio de venta es $0.00.
        /// </summary>
        public decimal MargenPorcentaje => PrecioVenta == 0 ? 0 : (GananciaNetaUnidad / PrecioVenta) * 100;
    }
}
using System;
using System.Collections.Generic;

namespace POS_Entidades
{
    public class Venta
    {
        public int IdVenta { get; set; }
        public string Folio { get; set; } = string.Empty;
        public DateTime FechaHora { get; set; }
        public decimal Total { get; set; }
        public int IdUsuario { get; set; }
        public List<VentaDetalle> Detalles { get; set; } = new List<VentaDetalle>();
        public string TipoOperacion { get; set; } = string.Empty;
        public int? IdCliente { get; set; }
        public string EstadoPago { get; set; } = string.Empty;

    }
}
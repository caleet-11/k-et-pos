using System;

namespace POS_Entidades
{
    public class ReporteVenta
    {
        public int IdVenta { get; set; }
        public string Folio { get; set; } = string.Empty;
        public DateTime Reloj { get; set; } // Reemplaza FechaHora para evitar conflictos
        public decimal IngresoBruto { get; set; }
        public decimal GananciaNeta { get; set; }
    }
}
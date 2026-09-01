using System;

namespace POS_Entidades
{
    public class ReporteDiario
    {
        public int FolioVenta { get; set; }
        public DateTime FechaHora { get; set; }
        public int ArticulosVendidos { get; set; }
        public decimal IngresoBruto { get; set; }  // Lo que pagó el cliente
        public decimal CostoTotal { get; set; }    // Lo que te costó a ti la mercancía

        // La magia ocurre aquí: La ganancia real se calcula sola
        public decimal GananciaNeta => IngresoBruto - CostoTotal;
    }
}
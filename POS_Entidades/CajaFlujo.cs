using System;

namespace POS_Entidades
{
    public enum TipoMovimientoCaja
    {
        Apertura,
        IngresoVenta,
        IngresoExtra,
        PagoProveedor,
        RetiroEfectivo,
        Cierre
    }

    public class CajaFlujo
    {
        public int IdMovimiento { get; set; }
        public int IdUsuario { get; set; }

        public TipoMovimientoCaja TipoMovimiento { get; set; }

        public decimal Monto { get; set; }

        // =======================================================
        // EL CAMBIO: Inicializamos con string.Empty para evitar nulos
        // =======================================================
        public string Motivo { get; set; } = string.Empty;

        public DateTime FechaHora { get; set; }
    }
}
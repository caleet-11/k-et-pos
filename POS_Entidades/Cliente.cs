using System.ComponentModel.DataAnnotations;

namespace POS_Entidades
{
    public class Cliente
    {
        public int IdCliente { get; set; }

        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Telefono { get; set; } = string.Empty;

        // =======================================================
        // NUEVOS DATOS DE FACTURACIÓN Y CONTACTO
        // =======================================================

        [MaxLength(100)]
        public string CorreoElectronico { get; set; } = string.Empty;

        [MaxLength(15)] // 13 es el máximo oficial, 15 da un margen seguro
        public string RFC { get; set; } = string.Empty;

        [MaxLength(255)]
        public string Direccion { get; set; } = string.Empty;

        // =======================================================

        public decimal LimiteCredito { get; set; }
        public decimal DeudaActual { get; set; }
        public bool Activo { get; set; }
    }
}
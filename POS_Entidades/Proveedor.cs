using System.ComponentModel.DataAnnotations;

namespace POS_Entidades
{
    public class Proveedor
    {
        public int IdProveedor { get; set; }

        [MaxLength(150)]
        public string Empresa { get; set; } = string.Empty;

        [MaxLength(100)]
        public string NombreContacto { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Telefono { get; set; } = string.Empty;

        public bool Activo { get; set; }
    }
}
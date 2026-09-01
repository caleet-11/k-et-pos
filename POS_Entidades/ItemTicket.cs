using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace POS_Entidades
{
    // 1. Heredamos de la interfaz mágica de WPF
    public class ItemTicket : INotifyPropertyChanged
    {
        // 2. Creamos el evento que avisa a la interfaz gráfica
        public event PropertyChangedEventHandler PropertyChanged;

        // Método auxiliar para disparar la alarma fácilmente
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Estas propiedades no cambian durante la venta, así que se quedan igual
        public int IdDetalle { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public decimal PrecioCosto { get; set; }

        // =========================================================================
        // VARIABLES REACTIVAS
        // =========================================================================

        private decimal precioVenta;
        public decimal PrecioVenta
        {
            get => precioVenta;
            set
            {
                if (precioVenta != value)
                {
                    precioVenta = value;
                    OnPropertyChanged(); // Avisa que cambió el precio
                    OnPropertyChanged(nameof(Subtotal)); // ¡Magia! Avisa que el Subtotal también debe recalcularse visualmente
                }
            }
        }

        private decimal cantidad;
        public decimal Cantidad
        {
            get => cantidad;
            set
            {
                if (cantidad != value)
                {
                    cantidad = value;
                    OnPropertyChanged(); // Avisa que cambió la cantidad
                    OnPropertyChanged(nameof(Subtotal)); // ¡Magia! Actualiza el subtotal en el DataGrid al instante
                }
            }
        }

        private decimal cantidadDevuelta;
        public decimal CantidadDevuelta
        {
            get => cantidadDevuelta;
            set
            {
                if (cantidadDevuelta != value)
                {
                    cantidadDevuelta = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(FueDevuelto)); // Actualiza la bandera visual si devuelven algo
                }
            }
        }

        // =========================================================================

        // El subtotal se calcula solo automáticamente
        public decimal Subtotal => Cantidad * PrecioVenta;

        // Bandera Visual para WPF
        public bool FueDevuelto => CantidadDevuelta > 0;
    }
}
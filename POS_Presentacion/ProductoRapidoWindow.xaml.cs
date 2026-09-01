using System;
using System.Windows;
using System.Windows.Input;

namespace POS_Presentacion
{
    public partial class ProductoRapidoWindow : Window
    {
        // Propiedades públicas para que VentasWindow pueda leer lo que el usuario escribió
        public string NombreProducto { get; private set; }
        public decimal PrecioProducto { get; private set; }

        public ProductoRapidoWindow()
        {
            InitializeComponent();
            txtNombre.Focus(); // Mandamos el cursor directo a la descripción
        }

        private void BtnAgregar_Click(object sender, RoutedEventArgs e)
        {
            // 1. Validamos que la descripción no esté vacía
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("Por favor, ingresa una descripción para el producto.", "Faltan Datos", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtNombre.Focus();
                return;
            }

            // OPTIMIZACIÓN: Limpiamos signos de pesos y también comas por si el usuario escribe "1,500.00"
            string precioLimpio = txtPrecio.Text.Replace("$", "").Replace(",", "").Trim();

            // 2. Validamos que el precio sea un número válido y mayor a cero
            if (decimal.TryParse(precioLimpio, out decimal precio) && precio > 0)
            {
                NombreProducto = txtNombre.Text.Trim();
                PrecioProducto = precio;

                // OPTIMIZACIÓN: WPF cierra la ventana automáticamente al asignar esta propiedad
                this.DialogResult = true;
            }
            else
            {
                MessageBox.Show("Por favor, ingresa un precio numérico válido y mayor a cero.", "Precio Inválido", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPrecio.Focus();
                txtPrecio.SelectAll();
            }
        }

        // Si presionan Enter en la caja del precio, se activa el botón de agregar automáticamente
        private void TxtPrecio_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // OPTIMIZACIÓN: Pasamos parámetros nulos para no cruzar tipos de eventos
                BtnAgregar_Click(null, null);
            }
        }
    }
}
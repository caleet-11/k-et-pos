using POS_Entidades;
using POS_Logica;
using System;
using System.Windows;

namespace POS_Presentacion
{
    public partial class MainWindow : Window
    {
        // OPTIMIZACIÓN: Mantenemos una sola instancia de la lógica y la blindamos con readonly
        private readonly ProductoLogica logica = new ProductoLogica();

        // Contador cíclico para productos sin código de fábrica
        private int contadorCiclicoInterno = 1000;

        public MainWindow()
        {
            InitializeComponent();

            // Al abrir la ventana, el cursor se pone listo en la caja de texto 
            // esperando el disparo del lector de barras
            txtCodigo.Focus();
        }

        private void BtnGenerarCodigo_Click(object sender, RoutedEventArgs e)
        {
            // Aumenta el contador cíclico estrictamente por cada clic
            contadorCiclicoInterno++;
            txtCodigo.Text = "INT-" + contadorCiclicoInterno.ToString();
        }

        private async void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 1. Recolectar los datos de la pantalla y armar la Entidad
                Producto nuevoProducto = new Producto
                {
                    Codigo = txtCodigo.Text,
                    Nombre = txtNombre.Text,
                    // Convertimos el texto a número decimal. Si falla, ponemos 0.
                    PrecioCosto = decimal.TryParse(txtPrecioCosto.Text, out decimal costo) ? costo : 0,
                    PrecioVenta = decimal.TryParse(txtPrecioVenta.Text, out decimal venta) ? venta : 0,
                    SeVendePorUnidad = chkSeVendePorUnidad.IsChecked ?? true,

                    // Stock inicial por defecto
                    Stock = 0,
                    StockMinimo = 5
                };

                // Llamamos a la lógica y nos devuelve true o false
                bool guardadoExitoso = await logica.RegistrarProductoAsync(nuevoProducto);

                if (guardadoExitoso)
                {
                    MessageBox.Show("Producto registrado correctamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                    // OPTIMIZACIÓN: Mandamos llamar a la función que ya tenías preparada
                    LimpiarPantalla();
                }
                else
                {
                    MessageBox.Show("Ocurrió un problema al guardar el producto", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error de formato: " + ex.Message, "Error Crítico", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LimpiarPantalla()
        {
            txtCodigo.Clear();
            txtNombre.Clear();
            txtPrecioCosto.Clear();
            txtPrecioVenta.Clear();
            chkSeVendePorUnidad.IsChecked = true;
            txtCodigo.Focus(); // Volvemos a preparar el lector
        }
    }
}
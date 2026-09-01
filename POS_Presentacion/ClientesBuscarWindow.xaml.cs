using POS_Entidades;
using POS_Logica;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace POS_Presentacion
{
    public partial class ClientesBuscarWindow : Window
    {
        private readonly ClienteLogica logica = new ClienteLogica();

        // Propiedad pública para que CobroWindow pueda leer a quién seleccionamos
        public Cliente ClienteSeleccionado { get; private set; }

        public ClientesBuscarWindow()
        {
            InitializeComponent();
            txtBuscar.Focus();
        }

        private async void TxtBuscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            string texto = txtBuscar.Text.Trim();

            // Para no saturar la base de datos, buscamos solo si escriben 2 letras o más
            if (texto.Length >= 2)
            {
                dgClientes.ItemsSource = await logica.BuscarClientesAsync(texto);
            }
            else
            {
                dgClientes.ItemsSource = null;
            }
        }

        private void BtnSeleccionar_Click(object sender, RoutedEventArgs e)
        {
            ConfirmarSeleccion();
        }

        private void DgClientes_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ConfirmarSeleccion();
        }

        private void ConfirmarSeleccion()
        {
            if (dgClientes.SelectedItem is Cliente cliente)
            {
                ClienteSeleccionado = cliente;
                this.DialogResult = true; // Avisamos que todo salió bien
                this.Close();
            }
            else
            {
                MessageBox.Show("Por favor, selecciona un cliente de la lista.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
using POS_Entidades;
using POS_Logica;
using System.Windows;
using System.Windows.Controls;

namespace POS_Presentacion
{
    public partial class ClientesWindow : Window
    {
        private readonly ClienteLogica logica = new ClienteLogica();
        private Cliente clienteSeleccionadoActual = null;

        public ClientesWindow()
        {
            InitializeComponent();
            _ = CargarClientesInicialesAsync();
        }

        private async System.Threading.Tasks.Task CargarClientesInicialesAsync()
        {
            // Carga todos los clientes al abrir la ventana enviando texto vacío o buscando "a"
            dgClientes.ItemsSource = await logica.BuscarClientesAsync("");
        }

        private async void TxtBuscarCliente_TextChanged(object sender, TextChangedEventArgs e)
        {
            string texto = txtBuscarCliente.Text.Trim();
            dgClientes.ItemsSource = await logica.BuscarClientesAsync(texto);
        }

        private async void DgClientes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgClientes.SelectedItem is Cliente cliente)
            {
                clienteSeleccionadoActual = cliente;

                // Actualizar interfaz derecha
                lblNombreCliente.Text = cliente.Nombre;
                lblDeudaTotal.Text = cliente.DeudaActual.ToString("C");

                // Habilitar botón de pago si hay deuda
                btnAbonar.IsEnabled = cliente.DeudaActual > 0;

                // Cargar tickets pendientes de forma asíncrona
                dgTickets.ItemsSource = await logica.ObtenerTicketsPendientesAsync(cliente.IdCliente);
            }
            else
            {
                // Limpiar la interfaz si no hay nada seleccionado
                clienteSeleccionadoActual = null;
                lblNombreCliente.Text = "Selecciona un cliente";
                lblDeudaTotal.Text = "$0.00";
                dgTickets.ItemsSource = null;
                btnAbonar.IsEnabled = false;
            }
        }

        private void BtnAbonar_Click(object sender, RoutedEventArgs e)
        {
            if (clienteSeleccionadoActual != null)
            {
                // Aquí abriremos la futura ventana para ingresar la cantidad de dinero a abonar
                MessageBox.Show($"Próximo paso: Abrir ventana de cobro para abonar a la deuda de {clienteSeleccionadoActual.DeudaActual:C}.",
                                "Módulo en Construcción", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private void BtnNuevoCliente_Click(object sender, RoutedEventArgs e)
        {
            // 1. Abrimos la ventana de registro en modo Modal
            RegistrarClienteWindow ventanaRegistro = new RegistrarClienteWindow();
            ventanaRegistro.ShowDialog();
        }
    }
}
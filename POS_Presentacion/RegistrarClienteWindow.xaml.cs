using POS_Entidades;
using POS_Datos;
using System;
using System.Windows;

namespace POS_Presentacion
{
    public partial class RegistrarClienteWindow : Window
    {
        // Instanciamos tu clase de conexión a datos
        private readonly ClienteDatos clienteDatos = new ClienteDatos();

        public RegistrarClienteWindow()
        {
            InitializeComponent();
        }

        private async void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            // 1. Validaciones de seguridad para evitar que la base de datos colapse
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("El nombre del cliente es obligatorio.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtNombre.Focus();
                return;
            }

            // Convertimos el texto del crédito a número decimal de forma segura
            if (!decimal.TryParse(txtLimiteCredito.Text, out decimal limiteCreditoValido))
            {
                MessageBox.Show("El límite de crédito debe ser un número válido (ej. 1500.50).", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtLimiteCredito.Focus();
                return;
            }

            // 2. Armamos el objeto con la información de los TextBox
            Cliente nuevoCliente = new Cliente
            {
                Nombre = txtNombre.Text.Trim(),
                Telefono = txtTelefono.Text.Trim(),
                CorreoElectronico = txtCorreo.Text.Trim(),
                RFC = txtRFC.Text.Trim(),
                Direccion = txtDireccion.Text.Trim(),
                LimiteCredito = limiteCreditoValido,
                DeudaActual = 0m, // Lógica estricta: deuda cero al iniciar
                Activo = chkActivo.IsChecked ?? true
            };

            // 3. Enviamos a guardar con manejo de errores (Try-Catch)
            try
            {
                // Bloqueamos el botón para que el usuario no le dé doble clic y lo guarde dos veces
                btnGuardar.IsEnabled = false;

                bool exito = await clienteDatos.InsertarClienteAsync(nuevoCliente);

                if (exito)
                {
                    MessageBox.Show("Cliente registrado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close(); // Cerramos la ventana automáticamente
                }
                else
                {
                    MessageBox.Show("Ocurrió un problema, no se pudo guardar en la base de datos.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    btnGuardar.IsEnabled = true; // Desbloqueamos por si quiere intentar de nuevo
                }
            }
            catch (Exception ex)
            {
                // Si la base de datos está caída o hay un error de conexión, el programa no se cierra de golpe
                MessageBox.Show($"Ocurrió un error crítico: {ex.Message}", "Error de Conexión", MessageBoxButton.OK, MessageBoxImage.Error);
                btnGuardar.IsEnabled = true;
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            // Simplemente cierra la ventana sin hacer nada
            this.Close();
        }
    }
}